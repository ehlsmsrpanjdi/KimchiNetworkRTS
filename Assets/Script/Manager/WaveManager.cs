using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : NetworkBehaviour
{
    public static WaveManager Instance;

    [Header("Wave Settings")]
    public Transform spawnPoint; // 몬스터 스폰 위치
    public int currentWaveID = 1;
    public float gameTime = 0f; // 게임 경과 시간 (분 단위 스케일링용)

    [Header("Wave State")]
    public NetworkVariable<int> currentWaveNumber = new NetworkVariable<int>(0);
    public NetworkVariable<bool> isWaveActive = new NetworkVariable<bool>(false);

    private WaveData currentWaveData;
    private Dictionary<int, int> spawnedCount = new Dictionary<int, int>(); // 몬스터ID별 스폰된 수
    private Dictionary<int, float> nextSpawnTime = new Dictionary<int, float>(); // 몬스터ID별 다음 스폰 시간
    private float waveStartTime;
    private bool bossSpawned = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!IsServer) return;

        // 게임 시간 업데이트
        gameTime += Time.deltaTime;

        // 웨이브 진행 중이면 몬스터 스폰
        if (isWaveActive.Value)
        {
            UpdateWaveSpawning();
        }
    }

    // ========== 웨이브 시작 ==========
    [Rpc(SendTo.Server)]
    public void StartWaveServerRpc()
    {
        if (!IsServer) return;

        if (isWaveActive.Value)
        {
            LogHelper.LogWarrning("Wave already active!");
            return;
        }

        // WaveData 로드
        currentWaveData = WaveDataManager.Instance.GetData(currentWaveID);
        if (currentWaveData == null)
        {
            LogHelper.LogError($"WaveData not found: {currentWaveID}");
            return;
        }

        // 웨이브 상태 초기화
        isWaveActive.Value = true;
        currentWaveNumber.Value = currentWaveData.waveNumber;
        waveStartTime = Time.time;
        bossSpawned = false;

        spawnedCount.Clear();
        nextSpawnTime.Clear();

        // 각 몬스터 타입별 초기화
        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
        foreach (var spawnInfo in currentWaveData.spawnInfos)
        {
            spawnedCount[spawnInfo.monsterID] = 0;
            nextSpawnTime[spawnInfo.monsterID] = Time.time;
        }

        LogHelper.Log($"✅ Wave {currentWaveData.waveNumber} started! (Duration: {currentWaveData.waveDuration}s, Players: {playerCount})");
    }

    // ========== 웨이브 스폰 업데이트 ==========
    void UpdateWaveSpawning()
    {
        float elapsedTime = Time.time - waveStartTime;
        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        bool allSpawned = true;

        // 각 몬스터 타입별로 스폰
        foreach (var spawnInfo in currentWaveData.spawnInfos)
        {
            int monsterID = spawnInfo.monsterID;
            int totalCount = currentWaveData.GetTotalSpawnCount(monsterID, playerCount);
            float spawnInterval = currentWaveData.GetSpawnInterval(monsterID, playerCount);

            // 아직 스폰할 몬스터가 남아있으면
            if (spawnedCount[monsterID] < totalCount)
            {
                allSpawned = false;

                // 스폰 시간이 되었으면
                if (Time.time >= nextSpawnTime[monsterID])
                {
                    SpawnMonster(monsterID);
                    spawnedCount[monsterID]++;
                    nextSpawnTime[monsterID] = Time.time + spawnInterval;

                    LogHelper.Log($"Spawned monster {monsterID} ({spawnedCount[monsterID]}/{totalCount})");
                }
            }
        }

        // 모든 일반 몬스터 스폰 완료 + 보스 있으면 보스 스폰
        if (allSpawned && currentWaveData.HasBoss() && !bossSpawned)
        {
            SpawnMonster(currentWaveData.bossMonsterID);
            bossSpawned = true;
            LogHelper.Log($"🔥 Boss spawned: {currentWaveData.bossMonsterID}");
        }

        // 스폰 완료 + 모든 몬스터 죽음 = 웨이브 종료
        if (allSpawned && (!currentWaveData.HasBoss() || bossSpawned))
        {
            if (MonsterManager.Instance.AreAllMonstersDead())
            {
                EndWave();
            }
        }
    }

    // ========== 몬스터 스폰 ==========
    void SpawnMonster(int monsterID)
    {
        MonsterData data = MonsterDataManager.Instance.GetData(monsterID);
        if (data == null)
        {
            LogHelper.LogError($"MonsterData not found: {monsterID}");
            return;
        }

        string prefabName = data.prefabName;
        GameObject prefab = AssetManager.Instance.GetByName(prefabName);
        if (prefab == null)
        {
            LogHelper.LogError($"Monster prefab not found: {prefabName}");
            return;
        }

        // 스폰 위치 (약간 랜덤)
        Vector3 spawnPos = spawnPoint.position + new Vector3(
            Random.Range(-2f, 2f),
            0f,
            Random.Range(-2f, 2f)
        );

        // Instantiate
        GameObject monsterGo = Instantiate(prefab, spawnPos, Quaternion.identity);
        var netObj = monsterGo.GetComponent<NetworkObject>();
        var monster = monsterGo.GetComponent<MonsterBase>();

        if (monster == null)
        {
            LogHelper.LogError($"MonsterBase missing on {prefabName}");
            Destroy(monsterGo);
            return;
        }

        // Initialize
        monster.Initialize(monsterID, gameTime);

        // Spawn
        netObj.Spawn();
    }

    // ========== 웨이브 종료 ==========
    void EndWave()
    {
        isWaveActive.Value = false;
        LogHelper.Log($"✅ Wave {currentWaveData.waveNumber} completed!");

        // ✅ 증강 선택 UI 표시
        AugmentManager.Instance.ShowAugmentSelectionServerRpc();

        // 다음 웨이브 준비
        currentWaveID++;

        // 30초 후 자동 시작
        StartCoroutine(StartNextWaveAfterDelay(30f));
    }


    IEnumerator StartNextWaveAfterDelay(float delay)
    {
        LogHelper.Log($"⏰ Next wave starts in {delay} seconds...");

        yield return new WaitForSeconds(delay);

        // 다음 웨이브 데이터 확인
        var nextWaveData = WaveDataManager.Instance.GetData(currentWaveID);
        if (nextWaveData != null)
        {
            LogHelper.Log($"🔥 Starting Wave {nextWaveData.waveNumber}!");
            StartWaveServerRpc();
        }
        else
        {
            LogHelper.Log($"🎉 All waves completed! Victory!");
        }
    }
}
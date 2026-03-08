using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : NetworkBehaviour
{
    public static WaveManager Instance;

    [Header("Wave Settings")]
    public Transform spawnPoint;
    public int currentWaveNumber = 1;
    public float gameTime = 0f;

    [Header("Card Selection Trigger")]
    [Tooltip("몇 웨이브마다 카드 선택 화면을 띄울지 (기본값 1 = 매 웨이브 종료마다)")]
    public int cardSelectEveryNWaves = 1;

    [Header("Wave State")]
    public NetworkVariable<int> currentWaveNum = new NetworkVariable<int>(0);
    public NetworkVariable<bool> isWaveActive = new NetworkVariable<bool>(false);

    private WaveData currentWaveData;
    private Dictionary<string, int> spawnedCount = new Dictionary<string, int>();
    private Dictionary<string, float> nextSpawnTime = new Dictionary<string, float>();
    private float waveStartTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!IsServer) return;
        gameTime += Time.deltaTime;
        if (isWaveActive.Value) UpdateWaveSpawning();
    }

    // ========== 웨이브 시작 ==========

    [Rpc(SendTo.Server)]
    public void StartWaveServerRpc()
    {
        if (!IsServer || isWaveActive.Value) return;

        currentWaveData = WaveDataManager.Instance.GetData(currentWaveNumber);
        if (currentWaveData == null)
        {
            LogHelper.LogError($"WaveData not found: wave {currentWaveNumber}");
            return;
        }

        isWaveActive.Value = true;
        currentWaveNum.Value = currentWaveData.waveNumber;
        waveStartTime = Time.time;

        spawnedCount.Clear();
        nextSpawnTime.Clear();

        foreach (var info in currentWaveData.spawnInfos)
        {
            spawnedCount[info.monsterID] = 0;
            nextSpawnTime[info.monsterID] = Time.time;
        }

        LogHelper.Log($"✅ Wave {currentWaveData.waveNumber} started!");
    }

    // ========== 스폰 업데이트 ==========

    void UpdateWaveSpawning()
    {
        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        // 몬스터 카드 "증식" 효과 누적 배율 적용
        float countMultiplier = CardManager.Instance.GetMonsterCountMultiplier();

        bool allSpawned = true;

        foreach (var info in currentWaveData.spawnInfos)
        {
            string monsterID = info.monsterID;

            int totalCount = Mathf.RoundToInt(
                (info.countBase + info.countPerPlayer * playerCount) * countMultiplier
            );

            float spawnInterval = info.spawnIntervalSec;

            if (spawnedCount[monsterID] < totalCount)
            {
                allSpawned = false;

                if (Time.time >= nextSpawnTime[monsterID])
                {
                    SpawnMonster(monsterID);
                    spawnedCount[monsterID]++;
                    nextSpawnTime[monsterID] = Time.time + spawnInterval;
                }
            }
        }

        if (allSpawned && MonsterManager.Instance.AreAllMonstersDead())
        {
            EndWave();
        }
    }

    // ========== 몬스터 스폰 ==========

    void SpawnMonster(string monsterID)
    {
        MonsterData data = MonsterDataManager.Instance.GetData(monsterID);
        if (data == null) { LogHelper.LogError($"MonsterData not found: {monsterID}"); return; }

        GameObject prefab = AssetManager.Instance.GetByName(data.prefabKey);
        if (prefab == null) { LogHelper.LogError($"Monster prefab not found: {data.prefabKey}"); return; }

        Vector3 spawnPos = spawnPoint.position + new Vector3(
            Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f)
        );

        GameObject monsterGo = Instantiate(prefab, spawnPos, Quaternion.identity);
        var netObj = monsterGo.GetComponent<NetworkObject>();
        var monster = monsterGo.GetComponent<MonsterBase>();

        if (monster == null) { LogHelper.LogError($"MonsterBase missing on {data.prefabKey}"); Destroy(monsterGo); return; }

        monster.Initialize(monsterID, currentWaveNumber);
        netObj.Spawn();
    }

    // ========== 웨이브 종료 ==========

    void EndWave()
    {
        isWaveActive.Value = false;
        LogHelper.Log($"✅ Wave {currentWaveData.waveNumber} completed!");

        // cardSelectEveryNWaves 웨이브마다 카드 선택
        // ex) cardSelectEveryNWaves = 3 → 3, 6, 9... 웨이브 종료 후 카드 선택
        bool shouldShowCards = (currentWaveNumber % cardSelectEveryNWaves == 0);

        if (shouldShowCards)
        {
            int cardChoices = currentWaveData.cardChoices;
            CardManager.Instance.OnWaveEndServerRpc(cardChoices);
            LogHelper.Log($"🃏 Card selection triggered (wave {currentWaveNumber})");
        }
        else
        {
            LogHelper.Log($"⏭ No card selection this wave (next at wave {GetNextCardWave()})");
        }

        currentWaveNumber++;
        StartCoroutine(StartNextWaveAfterDelay(shouldShowCards ? 30f : 10f));
    }

    /// <summary>
    /// 다음 카드 선택이 발생하는 웨이브 번호 반환 (로그/UI 표시용)
    /// </summary>
    int GetNextCardWave()
    {
        return (Mathf.FloorToInt((float)currentWaveNumber / cardSelectEveryNWaves) + 1) * cardSelectEveryNWaves;
    }

    IEnumerator StartNextWaveAfterDelay(float delay)
    {
        LogHelper.Log($"⏰ Next wave in {delay}s...");
        yield return new WaitForSeconds(delay);

        if (WaveDataManager.Instance.HasWave(currentWaveNumber))
        {
            StartWaveServerRpc();
        }
        else
        {
            LogHelper.Log("🎉 All waves completed! Victory!");
        }
    }
}

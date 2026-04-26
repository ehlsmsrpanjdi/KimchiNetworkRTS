using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 게임 시작 시 입구를 플레이어 수에 맞게 동적으로 조절
///
/// [동작 방식]
/// - 돌(Rock) 오브젝트마다 NavMeshObstacle(carving=true) 부착
/// - 입구를 열 때: obstacle.enabled = false → NavMesh가 해당 영역을 자동으로 뚫음
/// - 입구를 닫을 때: obstacle.enabled = true → NavMesh가 해당 영역을 막음
/// - NavMeshSurface.BuildNavMesh() 불필요 (carving이 실시간 처리)
///
/// [입구 계산]
/// - 전체 돌 N개를 일렬로 배치, 처음엔 전부 막힘
/// - 플레이어 추가 시 중앙부터 좌우 대칭으로 rocksPerPlayer개씩 열림
/// </summary>
public class EntranceManager : NetworkBehaviour
{
    public static EntranceManager Instance;

    [Header("Rock Grid")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform rockParent;
    [SerializeField] private int totalRocks = 10;
    [SerializeField] private float rockSpacing = 2f;

    [Header("Entrance Settings")]
    [Tooltip("플레이어 1명 추가 시 열리는 돌 개수 (짝수 권장 - 좌우 대칭)")]
    [SerializeField] private int rocksPerPlayer = 2;

    private List<RockEntry> rocks = new List<RockEntry>();
    private int lastPlayerCount = 0;
    private int currentOpenCount = 0;

    private class RockEntry
    {
        public GameObject gameObject;
        public NavMeshObstacle obstacle;
        public bool isOpen;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (!IsServer) return;

        //SpawnRocks();

        //NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoined;
        //NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerLeft;

        lastPlayerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
    }

    // warning 수정: override 추가
    public override void OnDestroy()
    {
        base.OnDestroy();

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerJoined;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerLeft;
        }
    }

    // ========== 플레이어 연결 이벤트 ==========

    void OnPlayerJoined(ulong clientId)
    {
        if (!IsServer) return;
        UpdateEntrance();
    }

    void OnPlayerLeft(ulong clientId)
    {
        if (!IsServer) return;
        // 나가도 입구는 줄이지 않음
    }

    // ========== 돌 스폰 ==========

    public void SpawnRocks()
    {
        if (rockParent == null)
            rockParent = new GameObject("RockParent").transform;

        float totalWidth = (totalRocks - 1) * rockSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < totalRocks; i++)
        {
            Vector3 pos = new Vector3(startX + i * rockSpacing, 0f, 0f);

            GameObject rockObj = Instantiate(rockPrefab, pos, Quaternion.identity, rockParent);
            rockObj.name = $"Rock_{i}";

            // NavMeshObstacle 부착 (없으면 자동 추가)
            NavMeshObstacle obstacle = rockObj.GetComponent<NavMeshObstacle>();
            if (obstacle == null)
                obstacle = rockObj.AddComponent<NavMeshObstacle>();

            // carving = true → NavMesh를 실시간으로 파서 몬스터가 이 칸을 못 지나감
            obstacle.carving = true;
            obstacle.carveOnlyStationary = true;
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Vector3.one * rockSpacing * 0.9f;
            obstacle.enabled = true;  // 기본값: 닫힘

            NetworkObject netObj = rockObj.GetComponent<NetworkObject>();
            if (netObj != null) netObj.Spawn();

            rocks.Add(new RockEntry
            {
                gameObject = rockObj,
                obstacle = obstacle,
                isOpen = false
            });
        }

        LogHelper.Log($"✅ Spawned {totalRocks} rocks with NavMeshObstacle(carving)");
    }

    // ========== 입구 업데이트 ==========

    public void UpdateEntrance()
    {
        if (!IsServer) return;

        //int currentPlayerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
        //if (currentPlayerCount <= lastPlayerCount) return;

        //int newPlayers = currentPlayerCount - lastPlayerCount;
        //int openCount = newPlayers * rocksPerPlayer;

        //OpenCenterRocks(openCount);

        //lastPlayerCount = currentPlayerCount;
        //LogHelper.Log($"🚪 Players: {currentPlayerCount} (+{newPlayers}), Opened +{openCount}, Total open: {currentOpenCount}/{totalRocks}");
    }

    /// <summary>
    /// 닫힌 돌들 중 중앙부터 좌우 대칭으로 count개 열기
    /// </summary>
    void OpenCenterRocks(int count)
    {
        if (count <= 0) return;

        // 현재 닫힌 돌의 인덱스 목록
        List<int> closedIndices = new List<int>();
        for (int i = 0; i < rocks.Count; i++)
        {
            if (!rocks[i].isOpen) closedIndices.Add(i);
        }

        if (closedIndices.Count == 0)
        {
            LogHelper.LogWarrning("No closed rocks left to open!");
            return;
        }

        // 닫힌 돌 기준 중앙부터 좌우 대칭으로 열기
        int center = closedIndices.Count / 2;
        int opened = 0;
        int offset = 0;

        while (opened < count)
        {
            int leftIdx = center - offset;
            int rightIdx = center + offset;

            bool leftValid = leftIdx >= 0 && leftIdx < closedIndices.Count;
            bool rightValid = rightIdx >= 0 && rightIdx < closedIndices.Count && rightIdx != leftIdx;

            if (!leftValid && !rightValid) break;

            if (leftValid && opened < count)
            {
                OpenRock(closedIndices[leftIdx]);
                opened++;
            }

            if (rightValid && opened < count)
            {
                OpenRock(closedIndices[rightIdx]);
                opened++;
            }

            offset++;
        }
    }

    void OpenRock(int rockIndex)
    {
        if (rockIndex < 0 || rockIndex >= rocks.Count) return;

        var rock = rocks[rockIndex];
        if (rock.isOpen) return;

        // obstacle 비활성화 → NavMesh가 이 위치를 자동으로 뚫음
        rock.obstacle.enabled = false;
        rock.isOpen = true;
        currentOpenCount++;

        LogHelper.Log($"🚪 Rock_{rockIndex} opened");
    }

    void CloseRock(int rockIndex)
    {
        if (rockIndex < 0 || rockIndex >= rocks.Count) return;

        var rock = rocks[rockIndex];
        if (!rock.isOpen) return;

        rock.obstacle.enabled = true;
        rock.isOpen = false;
        currentOpenCount--;

        LogHelper.Log($"🪨 Rock_{rockIndex} closed");
    }

    // ========== 외부 제어 ==========

    public void SetRockOpen(int rockIndex, bool open)
    {
        if (!IsServer) return;
        if (open) OpenRock(rockIndex);
        else CloseRock(rockIndex);
    }

    [ContextMenu("Open All")]
    public void OpenAll()
    {
        for (int i = 0; i < rocks.Count; i++) OpenRock(i);
    }

    [ContextMenu("Close All")]
    public void CloseAll()
    {
        for (int i = 0; i < rocks.Count; i++) CloseRock(i);
    }

    // ========== 디버그 ==========

    void OnDrawGizmosSelected()
    {
        if (rocks == null) return;
        foreach (var rock in rocks)
        {
            if (rock.gameObject == null) continue;
            Gizmos.color = rock.isOpen ? Color.green : Color.red;
            Gizmos.DrawWireCube(rock.gameObject.transform.position, Vector3.one * rockSpacing * 0.9f);
        }
    }
}
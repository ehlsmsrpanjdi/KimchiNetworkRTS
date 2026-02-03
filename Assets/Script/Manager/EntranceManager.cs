using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EntranceManager : NetworkBehaviour
{
    public static EntranceManager Instance;

    [Header("Rock Grid")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform rockParent;
    [SerializeField] private int totalRocks = 10;
    [SerializeField] private float rockSpacing = 2f;

    [Header("Entrance Settings")]
    [SerializeField] private int rocksPerPlayer = 2;  // 플레이어 1명당 제거할 돌 개수

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    private List<NetworkObject> rocks = new List<NetworkObject>();
    private int currentRemovedCount = 0;
    private int lastPlayerCount = 0;

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

    void Start()
    {
        if (IsServer)
        {
            SpawnRocks();

            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoined;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerLeft;

            lastPlayerCount = 1;  // Host
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerJoined;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerLeft;
        }
    }

    void OnPlayerJoined(ulong clientId)
    {
        if (!IsServer) return;
        UpdateEntranceWidth();
    }

    void OnPlayerLeft(ulong clientId)
    {
        if (!IsServer) return;
    }

    public void SpawnRocks()
    {
        if (rockParent == null)
        {
            GameObject parent = new GameObject("RockParent");
            rockParent = parent.transform;
        }

        float totalWidth = (totalRocks - 1) * rockSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < totalRocks; i++)
        {
            Vector3 position = new Vector3(
                startX + i * rockSpacing,
                0f,
                0f
            );

            GameObject rockObj = Instantiate(rockPrefab, position, Quaternion.identity, rockParent);
            rockObj.name = $"Rock_{i}";

            NetworkObject netObj = rockObj.GetComponent<NetworkObject>();

            if (netObj == null)
            {
                LogHelper.LogError($"NetworkObject missing on Rock prefab!");
                Destroy(rockObj);
                continue;
            }

            netObj.Spawn();
            rocks.Add(netObj);
        }

        LogHelper.Log($"✅ Spawned {totalRocks} rocks");
    }

    public void UpdateEntranceWidth()
    {
        if (!IsServer) return;

        int currentPlayerCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        // 플레이어 수가 증가했을 때만
        if (currentPlayerCount <= lastPlayerCount) return;

        // 추가된 플레이어 수
        int newPlayers = currentPlayerCount - lastPlayerCount;

        // 제거할 돌 개수
        int removeCount = newPlayers * rocksPerPlayer;

        if (removeCount > 0)
        {
            RemoveCenterRocks(removeCount);
            currentRemovedCount += removeCount;
            RebuildNavMeshServerRpc();
        }

        lastPlayerCount = currentPlayerCount;

        LogHelper.Log($"🪨 Players: {currentPlayerCount} (+{newPlayers}), Removed: {removeCount}, Total removed: {currentRemovedCount}");
    }

    /// <summary>
    /// 중앙부터 좌우 대칭으로 제거
    /// </summary>
    void RemoveCenterRocks(int count)
    {
        if (count <= 0 || count % 2 != 0)
        {
            LogHelper.LogError("count must be even number!");
            return;
        }

        int pairsToRemove = count / 2;  // 2개씩 제거하니까

        for (int i = 0; i < pairsToRemove; i++)
        {
            int n = rocks.Count / 2;  // 현재 남은 돌 기준 중앙

            int leftIndex = n - 1;
            int rightIndex = n;

            // 왼쪽 제거
            if (leftIndex >= 0 && leftIndex < rocks.Count)
            {
                NetworkObject leftRock = rocks[leftIndex];
                if (leftRock != null && leftRock.IsSpawned)
                {
                    leftRock.Despawn();
                    LogHelper.Log($"🪨 Removed Rock_{leftIndex}");
                }
            }

            // 오른쪽 제거
            if (rightIndex >= 0 && rightIndex < rocks.Count)
            {
                NetworkObject rightRock = rocks[rightIndex];
                if (rightRock != null && rightRock.IsSpawned)
                {
                    rightRock.Despawn();
                    LogHelper.Log($"🪨 Removed Rock_{rightIndex}");
                }
            }
        }
    }

    /// <summary>
    /// 중앙부터 좌우 교대로 인덱스 계산
    /// </summary>
    int GetNextRockIndex(int centerIndex, int iteration)
    {
        if (iteration == 0)
        {
            return centerIndex;
        }

        bool isLeft = iteration % 2 == 1;

        if (isLeft)
        {
            int offset = (iteration + 1) / 2;
            return centerIndex - offset;
        }
        else
        {
            int offset = iteration / 2;
            return centerIndex + offset;
        }
    }


    [Rpc(SendTo.Server)]
    void RebuildNavMeshServerRpc()
    {
        if (navMeshSurface == null)
        {
            navMeshSurface = FindFirstObjectByType<NavMeshSurface>();
        }

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            LogHelper.Log("✅ NavMesh rebuilt");
        }
    }

    [ContextMenu("Reset Entrance")]
    public void ResetEntrance()
    {
        LogHelper.LogWarrning("Reset not implemented - restart server to reset entrance");
    }
}
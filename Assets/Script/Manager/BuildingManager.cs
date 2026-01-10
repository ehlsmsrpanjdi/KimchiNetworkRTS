using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : NetworkBehaviour
{
    public static BuildingManager Instance;

    private Dictionary<ulong, List<BuildingBase>> playerBuildings = new Dictionary<ulong, List<BuildingBase>>();

    // ✅ 모든 건물 리스트 (빠른 접근용)
    private List<BuildingBase> allBuildings = new List<BuildingBase>();

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

    // ========== 건물 조회 ==========

    /// <summary>
    /// 모든 건물 가져오기 (읽기 전용)
    /// </summary>
    public List<BuildingBase> GetAllBuildings()
    {
        return allBuildings;
    }

    /// <summary>
    /// 살아있는 건물만 가져오기 (null 체크 + HP > 0)
    /// </summary>
    public List<BuildingBase> GetAliveBuildings()
    {
        return allBuildings.FindAll(b => b != null && b.gameObject.activeSelf && b.stat.currentHP.Value > 0);
    }

    /// <summary>
    /// 특정 플레이어의 건물 리스트
    /// </summary>
    public List<BuildingBase> GetPlayerBuildings(ulong playerID)
    {
        if (playerBuildings.TryGetValue(playerID, out List<BuildingBase> buildings))
        {
            return buildings;
        }
        return new List<BuildingBase>();
    }

    /// <summary>
    /// 주기적으로 파괴된 건물 정리 (선택사항)
    /// </summary>
    public void CleanupDestroyedBuildings()
    {
        allBuildings.RemoveAll(b => b == null || !b.gameObject.activeSelf);

        foreach (var kvp in playerBuildings)
        {
            kvp.Value.RemoveAll(b => b == null || !b.gameObject.activeSelf);
        }
    }

    // ========== 건물 배치 ==========

    [Rpc(SendTo.Server)]
    public void PlaceBuildingServerRpc(int buildingID, Vector3 worldPos, Vector2Int gridPos, ulong playerID)
    {
        if (!IsServer) return;

        BuildingData data = BuildingDataManager.Instance.GetData(buildingID);
        if (data == null)
        {
            LogHelper.LogError($"BuildingData not found: {buildingID}");
            return;
        }

        string prefabName = GetPrefabNameByID(buildingID);
        GameObject prefab = AssetManager.Instance.GetByName(prefabName);
        if (prefab == null)
        {
            LogHelper.LogError($"Building prefab not found: {prefabName}");
            return;
        }

        // 서버에서 직접 Instantiate
        GameObject buildingGo = Instantiate(prefab, worldPos, Quaternion.identity);
        buildingGo.name = prefabName;

        var netObj = buildingGo.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            LogHelper.LogError($"NetworkObject missing on {prefabName}");
            Destroy(buildingGo);
            return;
        }

        var buildingBase = buildingGo.GetComponent<BuildingBase>();
        if (buildingBase == null)
        {
            LogHelper.LogError($"BuildingBase missing on {prefabName}");
            Destroy(buildingGo);
            return;
        }

        // 초기화
        buildingBase.Initialize(buildingID, playerID, gridPos);

        // Spawn
        netObj.Spawn();

        // ✅ Player에 건물 등록
        Player ownerPlayer = PlayerManager.Instance.GetPlayer(playerID);
        if (ownerPlayer != null)
        {
            ownerPlayer.RegisterBuilding(buildingBase);
            LogHelper.Log($"✅ Building registered to Player {playerID}");
        }

        // ✅ 전체 리스트에 추가
        allBuildings.Add(buildingBase);

        // 플레이어별 리스트에 추가
        if (!playerBuildings.ContainsKey(playerID))
        {
            playerBuildings[playerID] = new List<BuildingBase>();
        }
        playerBuildings[playerID].Add(buildingBase);

        LogHelper.Log($"✅ Building placed: {data.displayName} at {gridPos} for player {playerID}");
    }

    // ========== 건물 삭제 ==========

    [Rpc(SendTo.Server)]
    public void RemoveBuildingServerRpc(NetworkObjectReference buildingRef, ulong playerID)
    {
        if (!IsServer) return;

        LogHelper.Log($"🔴 RemoveBuildingServerRpc called for player {playerID}");

        if (!buildingRef.TryGet(out NetworkObject netObj))
        {
            LogHelper.LogError("Failed to get NetworkObject from reference");
            return;
        }

        LogHelper.Log($"🔴 NetworkObject found: {netObj.gameObject.name}");

        var building = netObj.GetComponent<BuildingBase>();
        if (building == null)
        {
            LogHelper.LogError("BuildingBase not found on NetworkObject");
            return;
        }

        LogHelper.Log($"🔴 BuildingBase found, removing...");

        // ✅ Player에서 건물 제거
        Player ownerPlayer = PlayerManager.Instance.GetPlayer(playerID);
        if (ownerPlayer != null)
        {
            ownerPlayer.UnregisterBuilding(building);
            LogHelper.Log($"✅ Building unregistered from Player {playerID}");
        }

        // ✅ 전체 리스트에서 제거
        allBuildings.Remove(building);

        // 플레이어별 리스트에서 제거
        if (playerBuildings.ContainsKey(playerID))
        {
            playerBuildings[playerID].Remove(building);
            LogHelper.Log($"🔴 Removed from playerBuildings list");
        }

        // 네트워크 Despawn
        if (netObj.IsSpawned)
        {
            LogHelper.Log($"🔴 Calling Despawn...");
            netObj.Despawn(); // Handler가 풀 반환 처리
            LogHelper.Log($"🔴 Despawn called!");
        }
        else
        {
            LogHelper.LogError("NetworkObject is not spawned!");
        }

        LogHelper.Log($"✅ Building removed: {building.buildingID}");
    }

    // ========== Helper ==========

    string GetPrefabNameByID(int buildingID)
    {
        return buildingID switch
        {
            1 => "AttackTower",
            2 => "GoldMine",
            3 => "Wall",
            _ => "AttackTower"
        };
    }
}
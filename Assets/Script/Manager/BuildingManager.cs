using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : NetworkBehaviour
{
    public static BuildingManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // 서버에서만 쓰는 관리 리스트 (플레이어별 건물)
    private readonly Dictionary<ulong, List<BuildingBase>> playerBuildings = new();

    /// <summary>
    /// 건물 설치 (서버 RPC)
    /// </summary>
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

        buildingBase.Initialize(buildingID, playerID, gridPos);
        netObj.Spawn();

        // ✅ Player에 건물 등록
        Player ownerPlayer = PlayerManager.Instance.GetPlayer(playerID);
        if (ownerPlayer != null)
        {
            ownerPlayer.RegisterBuilding(buildingBase);
            LogHelper.Log($"✅ Building registered to Player {playerID}");
        }

        // 서버 관리 리스트에 추가
        if (!playerBuildings.ContainsKey(playerID))
        {
            playerBuildings[playerID] = new List<BuildingBase>();
        }
        playerBuildings[playerID].Add(buildingBase);

        LogHelper.Log($"✅ Building placed: {data.displayName} at {gridPos} for player {playerID}");
    }

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

        // 서버 관리 리스트에서 제거
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

    /// <summary>
    /// BuildingID → Prefab 이름 매핑
    /// ✅ TODO: 나중에 엑셀 데이터로 관리
    /// </summary>
    string GetPrefabNameByID(int buildingID)
    {
        switch (buildingID)
        {
            case 1: return "AttackTower";
            case 2: return "GoldMine";
            case 3: return "Wall";
            default:
                LogHelper.LogError($"Unknown buildingID: {buildingID}");
                return null;
        }
    }

    /// <summary>
    /// 플레이어의 건물 리스트 가져오기
    /// </summary>
    public List<BuildingBase> GetPlayerBuildings(ulong playerID)
    {
        if (playerBuildings.ContainsKey(playerID))
        {
            return playerBuildings[playerID];
        }
        return new List<BuildingBase>();
    }
}
using UnityEngine;
using UnityEditor;

public class DebugWindow : EditorWindow
{
    private Vector2Int gridPos = new Vector2Int(5, 5);

    [MenuItem("Tools/Debug Window")]
    public static void ShowWindow()
    {
        GetWindow<DebugWindow>("Debug Window");
    }

    void OnGUI()
    {
        GUILayout.Label("=== Grid Position ===", EditorStyles.boldLabel);
        gridPos.x = EditorGUILayout.IntField("Grid X:", gridPos.x);
        gridPos.y = EditorGUILayout.IntField("Grid Y:", gridPos.y);

        GUILayout.Space(10);

        // ========== 건물 배치 (직접) ==========
        GUILayout.Label("=== Place Building (Direct) ===", EditorStyles.boldLabel);

        if (GUILayout.Button("Place Attack Tower (1x1)"))
        {
            PlaceBuilding(1, gridPos);
        }

        if (GUILayout.Button("Place Gold Mine (2x2)"))
        {
            PlaceBuilding(2, gridPos);
        }

        if (GUILayout.Button("Place Wall (1x3)"))
        {
            PlaceBuilding(3, gridPos);
        }

        GUILayout.Space(10);

        // ========== Ghost 생성 ==========
        GUILayout.Label("=== Spawn Building Ghost ===", EditorStyles.boldLabel);

        if (GUILayout.Button("Spawn Attack Tower Ghost"))
        {
            SpawnGhost(1);
        }

        if (GUILayout.Button("Spawn Gold Mine Ghost"))
        {
            SpawnGhost(2);
        }

        if (GUILayout.Button("Spawn Wall Ghost"))
        {
            SpawnGhost(3);
        }

        GUILayout.Space(10);

        // ========== 건물 삭제 ==========
        GUILayout.Label("=== Delete Building ===", EditorStyles.boldLabel);

        if (GUILayout.Button("Delete Last Building"))
        {
            DeleteLastBuilding();
        }

        GUILayout.Label("=== Wave Manager ===", EditorStyles.boldLabel);

        if (GUILayout.Button("Start Wave"))
        {
            StartWave();
        }

        if (GUILayout.Button("Force End Wave"))
        {
            ForceEndWave();
        }
    }

    void PlaceBuilding(int buildingID, Vector2Int gridPos)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play 모드에서만 사용 가능합니다!");
            return;
        }

        BuildingData data = BuildingDataManager.Instance.GetData(buildingID);
        if (data == null)
        {
            Debug.LogError($"BuildingData not found: {buildingID}");
            return;
        }

        Vector3 worldPos = GridArea.Instance.GridToWorldWithSize(gridPos.x, gridPos.y, data.sizeX, data.sizeY);
        BuildingManager.Instance.PlaceBuildingServerRpc(buildingID, worldPos, gridPos, 0);
    }

    void SpawnGhost(int buildingID)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play 모드에서만 사용 가능합니다!");
            return;
        }

        // Ghost Prefab 이름 가져오기
        string ghostPrefabName = GetGhostPrefabName(buildingID);

        // AssetManager에서 로드 (Addressables)
        GameObject ghostPrefab = AssetManager.Instance.GetByName(ghostPrefabName);
        if (ghostPrefab == null)
        {
            Debug.LogError($"Ghost prefab not found: {ghostPrefabName}");
            return;
        }

        // Ghost 생성
        GameObject ghost = Object.Instantiate(ghostPrefab);
        var ghostScript = ghost.GetComponent<BuildingGhost>();
        if (ghostScript != null)
        {
            ghostScript.buildingID = buildingID;
            Debug.Log($"✅ Ghost spawned: {ghostPrefabName}");
        }
    }

    string GetGhostPrefabName(int buildingID)
    {
        return buildingID switch
        {
            1 => "AttackTowerGhost",
            2 => "GoldMineGhost",
            3 => "WallGhost",
            _ => "AttackTowerGhost"
        };
    }

    void DeleteLastBuilding()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play 모드에서만 사용 가능합니다!");
            return;
        }

        var playerBuildings = BuildingManager.Instance.GetPlayerBuildings(0);
        if (playerBuildings == null || playerBuildings.Count == 0)
        {
            Debug.LogWarning("No buildings to delete!");
            return;
        }

        var lastBuilding = playerBuildings[playerBuildings.Count - 1];
        var netObj = lastBuilding.GetComponent<Unity.Netcode.NetworkObject>();

        BuildingManager.Instance.RemoveBuildingServerRpc(netObj, 0);
        Debug.Log("Building deleted!");
    }

    void StartWave()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play 모드에서만 사용 가능합니다!");
            return;
        }

        WaveManager.Instance.StartWaveServerRpc();
    }

    void ForceEndWave()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play 모드에서만 사용 가능합니다!");
            return;
        }

        // TODO: WaveManager에 ForceEndWaveServerRpc 추가
    }

}
using TMPro;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;

public class DebugWindow : EditorWindow
{

    private BuildingBase lastPlacedBuilding; // 마지막 설치한 건물 저장


    TMP_FontAsset targetFont;
    private float debugFloatValue = 0f;
    private Vector2Int gridPos = new Vector2Int(5, 5); // 기본 그리드 위치

    [MenuItem("Window/DebugWindow")]
    public static void ShowWindow()
    {
        GetWindow<DebugWindow>("My Editor");
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Building Test", EditorStyles.boldLabel);

        // 그리드 위치 입력
        GUILayout.Label("Grid Position:");
        gridPos.x = EditorGUILayout.IntField("X:", gridPos.x);
        gridPos.y = EditorGUILayout.IntField("Y:", gridPos.y);

        GUILayout.Space(10);

        // 건물 설치 버튼들
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

        if (GUILayout.Button("Delete Last Building"))
        {
            DeleteLastBuilding();
        }


        GUILayout.Space(10);
        GUILayout.Label("DebugFloatValue", EditorStyles.boldLabel);
        debugFloatValue = EditorGUILayout.FloatField("입력값 : ", debugFloatValue);
    }

    void PlaceBuilding(int buildingID, Vector2Int gridPos)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Error", "Play 모드에서만 실행 가능합니다!", "OK");
            return;
        }

        if (BuildingManager.Instance == null)
        {
            Debug.LogError("BuildingManager.Instance is null!");
            return;
        }

        if (GridArea.Instance == null)
        {
            Debug.LogError("GridArea.Instance is null!");
            return;
        }

        // Grid 좌표 → World 좌표 변환
        BuildingData data = BuildingDataManager.Instance.GetData(buildingID);
        Vector3 worldPos = GridArea.Instance.GridToWorldWithSize(gridPos.x, gridPos.y, data.sizeX, data.sizeY);

        // 테스트용 플레이어 ID (0번 = 서버)
        ulong playerID = 0;

        // 건물 설치 RPC 호출
        BuildingManager.Instance.PlaceBuildingServerRpc(buildingID, worldPos, gridPos, playerID);

        Debug.Log($"Building {buildingID} placed at Grid({gridPos.x}, {gridPos.y}) World({worldPos})");
    }

    void DeleteLastBuilding()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Error", "Play 모드에서만 실행 가능합니다!", "OK");
            return;
        }

        // 서버에서 가장 최근 건물 찾아서 삭제
        if (BuildingManager.Instance == null) return;

        var buildings = BuildingManager.Instance.GetPlayerBuildings(0);
        if (buildings.Count > 0)
        {
            var lastBuilding = buildings[buildings.Count - 1];
            var netObjRef = new Unity.Netcode.NetworkObjectReference(lastBuilding.GetComponent<Unity.Netcode.NetworkObject>());
            BuildingManager.Instance.RemoveBuildingServerRpc(netObjRef, 0);

            Debug.Log("Building deleted!");
        }
        else
        {
            Debug.Log("No buildings to delete!");
        }


    }
}
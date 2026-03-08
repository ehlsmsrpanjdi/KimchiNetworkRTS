using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingButtonUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Button button;
    [SerializeField] Image iconImage;

    [Header("Building Data")]
    private string buildingID;
    private BuildingData data;

    void Reset()
    {
        button = GetComponent<Button>();
        iconImage = this.TryFindChild("iconImage").GetComponent<Image>();
    }

    void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// 건물 데이터로 초기화
    /// </summary>
    public void Initialize(string id)
    {
        buildingID = id;
        data = BuildingDataManager.Instance.GetData(buildingID);

        if (data == null)
        {
            LogHelper.LogError($"BuildingData not found: {buildingID}");
            return;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        // 비용 표시 (예: "Wood: 20, Iron: 50")
        string costString = "";
        for (int i = 0; i < data.constructionCosts.Length; i++)
        {
            var cost = data.constructionCosts[i];
            costString += $"{cost.resourceType}: {cost.amount}";
            if (i < data.constructionCosts.Length - 1)
                costString += ", ";
        }

        // 아이콘 (TODO: AssetManager에서 로드)
        // iconImage.sprite = ...
    }

    void OnClick()
    {
        Player localPlayer = PlayerManager.Instance.GetLocalPlayer();
        if (localPlayer == null)
        {
            LogHelper.LogError("Local player not found!");
            return;
        }

        // ✅ 자원 체크
        if (!localPlayer.resource.HasEnoughResources(data.constructionCosts))
        {
            LogHelper.LogWarrning("자원 부족!");
            // TODO: UI에 경고 표시
            return;
        }

        // ✅ BuildingGhost 생성
        SpawnBuildingGhost();
    }

    void SpawnBuildingGhost()
    {
        // prefabKey + "Ghost" 규칙 사용 (ex: "TowerBasic" → "TowerBasicGhost")
        string ghostPrefabName = data.prefabKey + "Ghost";
        GameObject ghostPrefab = AssetManager.Instance.GetByName(ghostPrefabName);

        if (ghostPrefab == null)
        {
            LogHelper.LogError($"Ghost prefab not found: {ghostPrefabName}");
            return;
        }

        GameObject ghostObj = Instantiate(ghostPrefab);
        BuildingGhost ghost = ghostObj.GetComponent<BuildingGhost>();

        if (ghost == null)
        {
            LogHelper.LogError($"BuildingGhost missing on {ghostPrefabName}");
            Destroy(ghostObj);
            return;
        }

        ghost.buildingID = buildingID;

        LogHelper.Log($"✅ BuildingGhost spawned: {data.displayName}");
    }

}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButtonsUI : UIBase
{
    [SerializeField] GridLayoutGroup gridLayoutGroup;
    [SerializeField] GameObject buildingButtonUIPrefab;
    [SerializeField] List<GameObject> buildingButtonUIs = new List<GameObject>();

    private void Reset()
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        buildingButtonUIPrefab = Resources.Load<GameObject>("BuildingButton");
    }

    /// <summary>
    /// 특정 카테고리의 건물들을 버튼으로 표시
    /// </summary>
    public void ShowBuildingsByCategory(BuildingCategory category)
    {
        RemoveAllButton();

        // BuildingDataManager에서 해당 카테고리의 건물 ID 가져오기
        List<int> buildingIDs = BuildingDataManager.Instance.GetBuildingIDsByCategory(category);

        foreach (int buildingID in buildingIDs)
        {
            AddButton(buildingID);
        }
    }

    void AddButton(int buildingID)
    {
        GameObject buttonObj = Instantiate(buildingButtonUIPrefab, gridLayoutGroup.transform);
        BuildingButtonUI button = buttonObj.GetComponent<BuildingButtonUI>();

        button.Initialize(buildingID);

        buildingButtonUIs.Add(buttonObj);
    }

    public void RemoveAllButton()
    {
        foreach (GameObject obj in buildingButtonUIs)
        {
            Destroy(obj);
        }
        buildingButtonUIs.Clear();
    }
}
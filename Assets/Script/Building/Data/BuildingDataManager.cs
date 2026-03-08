using System.Collections.Generic;
using UnityEngine;

public class BuildingDataManager
{
    static BuildingDataManager instance;
    public static BuildingDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new BuildingDataManager();
            }
            return instance;
        }
    }

    // ===== string ID 기반 딕셔너리 =====
    private Dictionary<string, BuildingData> dataDict = new Dictionary<string, BuildingData>();
    private Dictionary<BuildingCategory, List<BuildingData>> categoryDict = new Dictionary<BuildingCategory, List<BuildingData>>();


    // ========== 엑셀 데이터 로딩 ==========
    // 실제 프로젝트에서는 ScriptableObject 또는 JSON/CSV 파싱으로 교체 가능
    void GroupByCategory()
    {
        foreach (BuildingCategory cat in System.Enum.GetValues(typeof(BuildingCategory)))
        {
            categoryDict[cat] = new List<BuildingData>();
        }

        foreach (var data in dataDict.Values)
        {
            categoryDict[data.category].Add(data);
        }
    }

    public int Count => dataDict.Count;

    public void Clear()
    {
        dataDict.Clear();
        categoryDict.Clear();
        foreach (BuildingCategory cat in System.Enum.GetValues(typeof(BuildingCategory)))
            categoryDict[cat] = new System.Collections.Generic.List<BuildingData>();
    }

    public void Register(BuildingData data)
    {
        dataDict[data.buildingID] = data;
        if (!categoryDict.ContainsKey(data.category))
            categoryDict[data.category] = new System.Collections.Generic.List<BuildingData>();
        categoryDict[data.category].Add(data);
    }

    // ========== 조회 ==========

    public BuildingData GetData(string buildingID)
    {
        if (dataDict.TryGetValue(buildingID, out var data))
            return data;

        LogHelper.LogError($"BuildingData not found: {buildingID}");
        return null;
    }

    // int 인덱스로도 조회 가능 (기존 코드 호환용)
    public BuildingData GetData(int index)
    {
        int i = 0;
        foreach (var data in dataDict.Values)
        {
            if (i == index) return data;
            i++;
        }
        LogHelper.LogError($"BuildingData not found at index: {index}");
        return null;
    }

    public bool HasData(string buildingID) => dataDict.ContainsKey(buildingID);

    public List<BuildingData> GetByCategory(BuildingCategory category)
    {
        return categoryDict.TryGetValue(category, out var list) ? list : new List<BuildingData>();
    }

    // UI에서 사용하는 이름 (GetIDsByCategory의 별칭)
    public List<string> GetBuildingIDsByCategory(BuildingCategory category)
        => GetIDsByCategory(category);

    public List<string> GetIDsByCategory(BuildingCategory category)
    {
        var list = GetByCategory(category);
        var ids = new List<string>();
        foreach (var data in list)
            ids.Add(data.buildingID);
        return ids;
    }
}

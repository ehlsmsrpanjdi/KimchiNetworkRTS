using System.Collections.Generic;

public class BuildingUpgradeDataManager
{
    static BuildingUpgradeDataManager instance;
    public static BuildingUpgradeDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new BuildingUpgradeDataManager();
            }
            return instance;
        }
    }

    private Dictionary<string, BuildingUpgradeData> dataDict = new Dictionary<string, BuildingUpgradeData>();

    public void Register(BuildingUpgradeData data) => dataDict[data.upgradeID] = data;
    public void Clear() => dataDict.Clear();
    public int Count => dataDict.Count;

    public BuildingUpgradeData GetData(string upgradeID)
    {
        if (dataDict.TryGetValue(upgradeID, out var data))
            return data;

        LogHelper.LogError($"BuildingUpgradeData not found: {upgradeID}");
        return null;
    }

    /// <summary>
    /// 특정 건물에 적용 가능한 업그레이드 목록
    /// </summary>
    public List<BuildingUpgradeData> GetUpgradesForBuilding(string buildingID)
    {
        var result = new List<BuildingUpgradeData>();
        foreach (var data in dataDict.Values)
        {
            if (data.appliesToBuildingID == buildingID)
                result.Add(data);
        }
        return result;
    }
}

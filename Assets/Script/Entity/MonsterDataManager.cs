using System.Collections.Generic;

public class MonsterDataManager
{
    static MonsterDataManager instance;
    public static MonsterDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MonsterDataManager();
            }
            return instance;
        }
    }

    private Dictionary<string, MonsterData> dataDict = new Dictionary<string, MonsterData>();

    public void Register(MonsterData data) => dataDict[data.monsterID] = data;
    public void Clear() => dataDict.Clear();
    public int Count => dataDict.Count;

    public MonsterData GetData(string monsterID)
    {
        if (dataDict.TryGetValue(monsterID, out var data))
            return data;

        LogHelper.LogError($"MonsterData not found: {monsterID}");
        return null;
    }

    public bool HasData(string monsterID) => dataDict.ContainsKey(monsterID);

    public List<MonsterData> GetByArchetype(EnemyArchetype archetype)
    {
        var result = new List<MonsterData>();
        foreach (var data in dataDict.Values)
        {
            if (data.archetype == archetype)
                result.Add(data);
        }
        return result;
    }
}

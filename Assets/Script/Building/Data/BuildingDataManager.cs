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
                instance.Initialize();
            }
            return instance;
        }
    }

    private Dictionary<int, BuildingData> dataDict = new Dictionary<int, BuildingData>();

    void Initialize()
    {
        LoadTestData();
    }

    void LoadTestData()
    {
        // ID 1: 기본 포탑
        var attackTower = new BuildingData
        {
            buildingID = 1,
            displayName = "기본 포탑",
            category = BuildingCategory.Attack,
            sizeX = 1,
            sizeY = 1,
            constructionCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Wood, amount = 20 },
                new ResourceCost { resourceType = ResourceType.Iron, amount = 50 },
            },
            baseMaxHP = 500,
            isAttackTower = true,
            baseAttackDamage = 25,
            baseAttackSpeed = 1f,
            baseAttackRange = 10f,
            attackPriority = AttackPriority.Nearest,
            bulletPrefabID = 1,
            bulletSpeed = 15f,
            bulletMovementID = 1
        };

        // ID 2: 철 광산
        var ironMine = new BuildingData
        {
            buildingID = 2,
            displayName = "철 광산",
            category = BuildingCategory.Resource,
            sizeX = 2,
            sizeY = 2,
            constructionCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Iron, amount = 50 }
            },
            baseMaxHP = 300,
            baseAttackDamage = 100,    // 최대 스택
            baseAttackSpeed = 5,       // 초당 5씩 획득
            baseAttackRange = 3f,      // 수확 범위
            resourceType = ResourceType.Iron,
            harvestDuration = 2f
        };

        // ID 3: 벽
        var wall = new BuildingData
        {
            buildingID = 3,
            displayName = "벽",
            category = BuildingCategory.Wall,
            sizeX = 1,
            sizeY = 3,
            constructionCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Iron, amount = 80 }
            },
            baseMaxHP = 1000,
            baseDefense = 50,
            baseAttackRange = 3f,   // 수리 범위
            baseAttackSpeed = 10f   // 초당 수리량
        };

        // ID 4: 나무 농장
        var woodFarm = new BuildingData
        {
            buildingID = 4,
            displayName = "나무 농장",
            category = BuildingCategory.Resource,
            sizeX = 2,
            sizeY = 2,
            constructionCosts = new ResourceCost[]
            {
                new ResourceCost { resourceType = ResourceType.Wood, amount = 30 }
            },
            baseMaxHP = 300,
            baseAttackDamage = 100,    // 최대 스택
            baseAttackSpeed = 10,      // 초당 10씩 획득
            baseAttackRange = 3f,      // 수확 범위
            resourceType = ResourceType.Wood,
            harvestDuration = 2f
        };

        dataDict[1] = attackTower;
        dataDict[2] = ironMine;
        dataDict[3] = wall;
        dataDict[4] = woodFarm;
    }

    public BuildingData GetData(int buildingID)
    {
        if (dataDict.TryGetValue(buildingID, out var data))
        {
            return data;
        }

        LogHelper.LogError($"BuildingData not found: ID {buildingID}");
        return null;
    }

    public bool HasData(int buildingID)
    {
        return dataDict.ContainsKey(buildingID);
    }

    /// <summary>
    /// 특정 카테고리의 모든 건물 ID 가져오기
    /// </summary>
    public List<int> GetBuildingIDsByCategory(BuildingCategory category)
    {
        List<int> result = new List<int>();

        foreach (var kvp in dataDict)
        {
            if (kvp.Value.category == category)
            {
                result.Add(kvp.Key);
            }
        }

        return result;
    }
}
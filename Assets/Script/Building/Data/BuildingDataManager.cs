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
        // ✅ TODO: 나중에 엑셀에서 로드
        // 지금은 하드코딩으로 테스트 데이터
        LoadTestData();
    }

    void LoadTestData()
    {
        // ID 1: 기본 포탑 (골드 50, 나무 20)
        var attackTower = new BuildingData
        {
            buildingID = 1,
            displayName = "기본 포탑",
            category = BuildingCategory.Attack,
            sizeX = 1,
            sizeY = 1,
            constructionCosts = new ResourceCost[]
            {
            new ResourceCost { resourceType = ResourceType.Gold, amount = 50 },
            new ResourceCost { resourceType = ResourceType.Wood, amount = 20 }
            },
            baseMaxHP = 500,
            isAttackTower = true,
            baseAttackDamage = 25
        };

        // ID 2: 금광 (골드 100, 나무 50, 돌 30)
        var goldMine = new BuildingData
        {
            buildingID = 2,
            displayName = "금광",
            category = BuildingCategory.Resource,
            sizeX = 2,
            sizeY = 2,
            constructionCosts = new ResourceCost[]
            {
            new ResourceCost { resourceType = ResourceType.Gold, amount = 100 },
            new ResourceCost { resourceType = ResourceType.Wood, amount = 50 },
            new ResourceCost { resourceType = ResourceType.Stone, amount = 30 }
            },
            baseMaxHP = 300,
            resourceType = ResourceType.Gold,
            baseResourceRate = 10
        };

        // ID 3: 벽 (돌 80)
        var wall = new BuildingData
        {
            buildingID = 3,
            displayName = "벽",
            category = BuildingCategory.Wall,
            sizeX = 1,
            sizeY = 3,
            constructionCosts = new ResourceCost[]
            {
            new ResourceCost { resourceType = ResourceType.Stone, amount = 80 }
            },
            baseMaxHP = 1000
        };

        dataDict[1] = attackTower;
        dataDict[2] = goldMine;
        dataDict[3] = wall;
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
}
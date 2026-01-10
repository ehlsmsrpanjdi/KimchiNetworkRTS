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
                instance.LoadTestData();
            }
            return instance;
        }
    }

    private Dictionary<int, MonsterData> dataDict = new Dictionary<int, MonsterData>();

    void LoadTestData()
    {
        // ID 1: 기본 근접 몬스터
        var meleeBasic = new MonsterData
        {
            monsterID = 1,
            displayName = "좀비",
            monsterType = MonsterType.Melee,
            description = "느리지만 체력이 높은 근접 몬스터",
            prefabName = "ZombieMelee",
            baseMaxHP = 100,
            baseDefense = 5,
            baseMoveSpeed = 3f,
            baseAttackDamage = 10,
            baseAttackRange = 2f, // 근접 고정
            baseAttackSpeed = 1f, // 초당 1회
            scalingInterval = 1f, // 1분마다
            hpScaling = 0.15f,    // 15% 증가
            damageScaling = 0.1f  // 10% 증가
        };

        // ID 2: 빠른 원거리 몬스터
        var rangedFast = new MonsterData
        {
            monsterID = 2,
            displayName = "궁수 스켈레톤",
            monsterType = MonsterType.Ranged,
            description = "빠르고 원거리 공격을 하지만 체력이 낮음",
            prefabName = "SkeletonArcher",
            baseMaxHP = 50,
            baseDefense = 2,
            baseMoveSpeed = 5f,
            baseAttackDamage = 15,
            baseAttackRange = 8f,
            baseAttackSpeed = 0.5f, // 2초당 1회
            scalingInterval = 1f,
            hpScaling = 0.1f,
            damageScaling = 0.12f
        };

        // ID 6: 보스 몬스터
        var boss = new MonsterData
        {
            monsterID = 6,
            displayName = "좀비 왕",
            monsterType = MonsterType.Boss,
            description = "강력한 보스 몬스터",
            prefabName = "ZombieKing",
            baseMaxHP = 1000,
            baseDefense = 20,
            baseMoveSpeed = 2f,
            baseAttackDamage = 50,
            baseAttackRange = 3f,
            baseAttackSpeed = 0.5f,
            scalingInterval = 1f,
            hpScaling = 0.2f,
            damageScaling = 0.15f,
            isBoss = true
        };

        dataDict[1] = meleeBasic;
        dataDict[2] = rangedFast;
        dataDict[6] = boss;
    }

    public MonsterData GetData(int monsterID)
    {
        if (dataDict.TryGetValue(monsterID, out MonsterData data))
        {
            return data;
        }
        LogHelper.LogError($"MonsterData not found: {monsterID}");
        return null;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// StreamingAssets/Data/ 폴더의 CSV 파일들을 읽어 각 DataManager에 로드
/// 
/// [사용법]
/// GameManager 또는 LoadManager에서 게임 시작 전 호출:
///     ExcelDataLoader.Instance.LoadAll();
/// 
/// [CSV 파일 위치]
///     Assets/StreamingAssets/Data/Buildings.csv
///     Assets/StreamingAssets/Data/Enemies.csv
///     Assets/StreamingAssets/Data/Cards_Building.csv
///     Assets/StreamingAssets/Data/Cards_Monster.csv
///     Assets/StreamingAssets/Data/Waves.csv
///     Assets/StreamingAssets/Data/BuildingUpgrades.csv
/// 
/// [인코딩]
///     CSV 저장 시 반드시 UTF-8로 저장 (엑셀: '파일→다른이름으로저장→CSV UTF-8')
/// </summary>
public class ExcelDataLoader
{
    static ExcelDataLoader instance;
    public static ExcelDataLoader Instance
    {
        get { return instance ?? (instance = new ExcelDataLoader()); }
    }

    static readonly string DataPath = Path.Combine(Application.streamingAssetsPath, "Data");

    // ========== 전체 로드 ==========

    public void LoadAll()
    {
        LoadBuildings();
        LoadEnemies();
        LoadPlayerCards();
        LoadMonsterCards();
        LoadWaves();
        LoadBuildingUpgrades();

        LogHelper.Log("✅ ExcelDataLoader: All CSV data loaded");
    }

    // ========== Buildings.csv ==========
    // BuildingId, Name, Category, SizeX, SizeY, BuildCost_ResourceId, BuildCost,
    // MaxHP, Armor, CanAttack, Damage, DamageType, Range, FireRate, FireCount,
    // TargetingRule, SplashRadius, BulletSpeed, BulletMovement ID,
    // ResourceGen_ResourceId, TradeEnabled, PrefabKey, BulletPrefabKey, IconKey, Description

    void LoadBuildings()
    {
        var rows = ReadCSV("Buildings.csv");
        if (rows == null) return;

        BuildingDataManager.Instance.Clear();

        foreach (var row in rows)
        {
            try
            {
                var data = new BuildingData
                {
                    buildingID          = row["BuildingId"],
                    displayName         = row["Name"],
                    category            = ParseEnum<BuildingCategory>(row["Category"]),
                    sizeX               = ParseInt(row["SizeX"]),
                    sizeY               = ParseInt(row["SizeY"]),
                    baseMaxHP           = ParseFloat(row["MaxHP"]),
                    baseArmor           = ParseFloat(row["Armor"]),
                    canAttack           = ParseBool(row["CanAttack"]),
                    baseDamage          = ParseFloat(row["Damage"]),
                    damageType          = ParseEnum<DamageType>(row["DamageType"], DamageType.None),
                    baseRange           = ParseFloat(row["Range"]),
                    baseFireRate        = ParseFloat(row["FireRate"]),
                    baseFireCount       = ParseInt(row["FireCount"]),
                    targetingRule       = ParseEnum<TargetingRule>(row["TargetingRule"], TargetingRule.Nearest),
                    splashRadius        = ParseFloat(row["SplashRadius"]),
                    bulletSpeed         = ParseFloat(row["BulletSpeed"]),
                    bulletMovementType  = ParseEnum<BulletMovementType>(row["BulletMovement ID"], BulletMovementType.Straight),
                    resourceGenType     = ParseEnum<ResourceType>(row["ResourceGen_ResourceId"], ResourceType.Iron),
                    tradeEnabled        = ParseBool(row["TradeEnabled"]),
                    prefabKey           = row["PrefabKey"],
                    bulletPrefabKey     = row["BulletPrefabKey"],
                    iconKey             = row["IconKey"],
                    description         = row["Description"],
                    constructionCosts   = ParseResourceCosts(row["BuildCost_ResourceId"], row["BuildCost"])
                };

                BuildingDataManager.Instance.Register(data);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"Buildings.csv parse error [{row["BuildingId"]}]: {e.Message}");
            }
        }

        LogHelper.Log($"✅ Buildings loaded: {BuildingDataManager.Instance.Count}");
    }

    // ========== Enemies.csv ==========
    // EnemyId, Name, Archetype, MaxHP, Armor, MoveSpeed, AttackDamage,
    // AttackRange, AttackRate, Reward_ResourceId, RewardAmount, StatRate,
    // PrefabKey, IconKey, Description

    void LoadEnemies()
    {
        var rows = ReadCSV("Enemies.csv");
        if (rows == null) return;

        MonsterDataManager.Instance.Clear();

        foreach (var row in rows)
        {
            try
            {
                var data = new MonsterData
                {
                    monsterID           = row["EnemyId"],
                    displayName         = row["Name"],
                    archetype           = ParseEnum<EnemyArchetype>(row["Archetype"]),
                    baseMaxHP           = ParseFloat(row["MaxHP"]),
                    baseArmor           = ParseFloat(row["Armor"]),
                    baseMoveSpeed       = ParseFloat(row["MoveSpeed"]),
                    baseAttackDamage    = ParseFloat(row["AttackDamage"]),
                    baseAttackRange     = ParseFloat(row["AttackRange"]),
                    baseAttackRate      = ParseFloat(row["AttackRate"]),
                    rewardResourceType  = ParseEnum<ResourceType>(row["Reward_ResourceId"], ResourceType.Iron),
                    rewardAmount        = ParseInt(row["RewardAmount"]),
                    statRate            = ParseFloat(row["StatRate"]),
                    prefabKey           = row["PrefabKey"],
                    iconKey             = row["IconKey"],
                    description         = row["Description"]
                };

                MonsterDataManager.Instance.Register(data);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"Enemies.csv parse error [{row["EnemyId"]}]: {e.Message}");
            }
        }

        LogHelper.Log($"✅ Enemies loaded: {MonsterDataManager.Instance.Count}");
    }

    // ========== Cards_Building.csv ==========
    // CardId, Name, Rarity, Weight, EffectType, StatKey, Op, Value, EffectID, IconKey, Description

    void LoadPlayerCards()
    {
        var rows = ReadCSV("Cards_Building.csv");
        if (rows == null) return;

        CardDataManager.Instance.ClearPlayerCards();

        foreach (var row in rows)
        {
            try
            {
                var data = new PlayerCardData
                {
                    cardID      = row["CardId"],
                    displayName = row["Name"],
                    rarity      = ParseEnum<CardRarity>(row["Rarity"]),
                    weight      = ParseInt(row["Weight"]),
                    effectType  = ParseEnum<CardEffectType>(row["EffectType"]),
                    statKey     = row["StatKey"],
                    op          = ParseEnum<CardOp>(row["Op"]),
                    value       = ParseFloat(row["Value"]),
                    effectID    = row["EffectID"],
                    iconKey     = row["IconKey"],
                    description = row["Description"]
                };

                CardDataManager.Instance.RegisterPlayer(data);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"Cards_Building.csv parse error [{row["CardId"]}]: {e.Message}");
            }
        }

        LogHelper.Log($"✅ PlayerCards loaded: {CardDataManager.Instance.PlayerCardCount}");
    }

    // ========== Cards_Monster.csv ==========
    // CardId, Name, Rarity, Weight, EffectType, StatKey, Op, Value, EffectID, IconKey, Description

    void LoadMonsterCards()
    {
        var rows = ReadCSV("Cards_Monster.csv");
        if (rows == null) return;

        CardDataManager.Instance.ClearMonsterCards();

        foreach (var row in rows)
        {
            try
            {
                var data = new MonsterCardData
                {
                    cardID      = row["CardId"],
                    displayName = row["Name"],
                    rarity      = ParseEnum<CardRarity>(row["Rarity"]),
                    weight      = ParseInt(row["Weight"]),
                    effectType  = ParseEnum<CardEffectType>(row["EffectType"]),
                    statKey     = row["StatKey"],
                    op          = ParseEnum<CardOp>(row["Op"]),
                    value       = ParseFloat(row["Value"]),
                    effectID    = row["EffectID"],
                    iconKey     = row["IconKey"],
                    description = row["Description"]
                };

                CardDataManager.Instance.RegisterMonster(data);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"Cards_Monster.csv parse error [{row["CardId"]}]: {e.Message}");
            }
        }

        LogHelper.Log($"✅ MonsterCards loaded: {CardDataManager.Instance.MonsterCardCount}");
    }

    // ========== Waves.csv ==========
    // WaveId, WaveNumber, EnemyId, CountBase, CountPerPlayer,
    // SpawnDurationSec, SpawnIntervalSec, IsBossWave, CardMonsterChoices, Notes
    // 같은 WaveNumber 여러 행 → 하나의 WaveData로 병합

    void LoadWaves()
    {
        var rows = ReadCSV("Waves.csv");
        if (rows == null) return;

        WaveDataManager.Instance.Clear();

        // AddRow는 buffer Dictionary를 공유해서 같은 waveNumber를 병합
        var buffer = new Dictionary<int, WaveData>();

        foreach (var row in rows)
        {
            try
            {
                int waveNumber  = ParseInt(row["WaveNumber"]);
                int cardChoices = ParseInt(row["CardMonsterChoices"]);
                bool isBossWave = ParseBool(row["IsBossWave"]);

                WaveDataManager.Instance.AddRow(
                    buffer,
                    waveNumber,
                    row["EnemyId"],
                    ParseInt(row["CountBase"]),
                    ParseInt(row["CountPerPlayer"]),
                    ParseFloat(row["SpawnDurationSec"]),
                    ParseFloat(row["SpawnIntervalSec"]),
                    isBossWave,
                    cardChoices
                );
            }
            catch (Exception e)
            {
                LogHelper.LogError($"Waves.csv parse error [{row["WaveId"]}]: {e.Message}");
            }
        }

        // buffer를 WaveDataManager에 등록
        WaveDataManager.Instance.LoadFromBuffer(buffer);

        LogHelper.Log($"✅ Waves loaded: {WaveDataManager.Instance.Count} waves");
    }

    // ========== BuildingUpgrades.csv ==========
    // UpgradeId, Name, AppliesToBuildingId, Tier, Cost_ResourceId, Cost,
    // MaxStacks, PrereqUpgradeId, MutatorType, StatKey, Op, Value,
    // ExclusiveGroup, IconKey, Description

    void LoadBuildingUpgrades()
    {
        var rows = ReadCSV("BuildingUpgrades.csv");
        if (rows == null) return;

        BuildingUpgradeDataManager.Instance.Clear();

        foreach (var row in rows)
        {
            try
            {
                var data = new BuildingUpgradeData
                {
                    upgradeID               = row["UpgradeId"],
                    displayName             = row["Name"],
                    appliesToBuildingID     = row["AppliesToBuildingId"],
                    tier                    = ParseInt(row["Tier"]),
                    costResourceType        = ParseEnum<ResourceType>(row["Cost_ResourceId"], ResourceType.Iron),
                    cost                    = ParseInt(row["Cost"]),
                    maxStacks               = ParseInt(row["MaxStacks"]),
                    prereqUpgradeID         = row["PrereqUpgradeId"],
                    mutatorType             = ParseEnum<UpgradeMutatorType>(row["MutatorType"]),
                    statKey                 = row["StatKey"],
                    op                      = ParseEnum<CardOp>(row["Op"]),
                    value                   = ParseFloat(row["Value"]),
                    exclusiveGroup          = row["ExclusiveGroup"],
                    iconKey                 = row["IconKey"],
                    description             = row["Description"]
                };

                BuildingUpgradeDataManager.Instance.Register(data);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"BuildingUpgrades.csv parse error [{row["UpgradeId"]}]: {e.Message}");
            }
        }

        LogHelper.Log($"✅ BuildingUpgrades loaded: {BuildingUpgradeDataManager.Instance.Count}");
    }

    // ========== CSV 파일 읽기 ==========

    List<Dictionary<string, string>> ReadCSV(string fileName)
    {
        string path = Path.Combine(DataPath, fileName);

        if (!File.Exists(path))
        {
            LogHelper.LogError($"CSV not found: {path}");
            return null;
        }

        var result = new List<Dictionary<string, string>>();
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        if (lines.Length < 2)
        {
            LogHelper.LogWarrning($"CSV empty or header only: {fileName}");
            return result;
        }

        // 헤더 파싱
        string[] headers = SplitCSVLine(lines[0]);

        // 데이터 행 파싱
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = SplitCSVLine(line);
            var row = new Dictionary<string, string>();

            for (int j = 0; j < headers.Length; j++)
            {
                string header = headers[j].Trim();
                string value = j < values.Length ? values[j].Trim() : "";
                row[header] = value;
            }

            result.Add(row);
        }

        return result;
    }

    // 쉼표 분리 (쌍따옴표 안의 쉼표 무시)
    string[] SplitCSVLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());

        return result.ToArray();
    }

    // ========== 파싱 헬퍼 ==========

    int ParseInt(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        return int.TryParse(s, out int v) ? v : 0;
    }

    float ParseFloat(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0f;
        return float.TryParse(s, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : 0f;
    }

    bool ParseBool(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        return s.Trim().ToUpper() == "TRUE";
    }

    T ParseEnum<T>(string s, T defaultValue = default) where T : struct
    {
        if (string.IsNullOrEmpty(s)) return defaultValue;
        return Enum.TryParse<T>(s.Trim(), true, out T v) ? v : defaultValue;
    }

    ResourceCost[] ParseResourceCosts(string resourceId, string amount)
    {
        if (string.IsNullOrEmpty(resourceId) || string.IsNullOrEmpty(amount))
            return new ResourceCost[0];

        return new ResourceCost[]
        {
            new ResourceCost
            {
                resourceType = ParseEnum<ResourceType>(resourceId, ResourceType.Iron),
                amount = ParseInt(amount)
            }
        };
    }
}

using System.Collections.Generic;
using System.Linq;

public class AugmentDataManager
{
    static AugmentDataManager instance;
    public static AugmentDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AugmentDataManager();
                instance.Initialize();
            }
            return instance;
        }
    }

    private Dictionary<int, AugmentData> dataDict = new Dictionary<int, AugmentData>();
    private Dictionary<AugmentRarity, List<AugmentData>> rarityDict = new Dictionary<AugmentRarity, List<AugmentData>>();

    void Initialize()
    {
        LoadTestData();
        GroupByRarity();
    }

    void LoadTestData()
    {
        // ===== Silver 증강 (ID 1~3) =====
        dataDict[1] = new AugmentData
        {
            augmentID = 1,
            displayName = "공격력 강화",
            description = "공격력이 10 증가합니다 (중복: +3)",
            rarity = AugmentRarity.Silver,
            targetStat = AugmentTargetStat.AttackDamage,
            modifierType = StatModifier.ModifierType.Additive,
            firstValue = 10f,
            stackValue = 3f
        };

        dataDict[2] = new AugmentData
        {
            augmentID = 2,
            displayName = "공격속도 강화",
            description = "공격속도가 0.2 증가합니다 (중복: +0.1)",
            rarity = AugmentRarity.Silver,
            targetStat = AugmentTargetStat.AttackSpeed,
            modifierType = StatModifier.ModifierType.Additive,
            firstValue = 0.2f,
            stackValue = 0.1f
        };

        dataDict[3] = new AugmentData
        {
            augmentID = 3,
            displayName = "사거리 증가",
            description = "공격 사거리가 2 증가합니다 (중복: +1)",
            rarity = AugmentRarity.Silver,
            targetStat = AugmentTargetStat.AttackRange,
            modifierType = StatModifier.ModifierType.Additive,
            firstValue = 2f,
            stackValue = 1f
        };

        // ===== Gold 증강 (ID 4~6) =====
        dataDict[4] = new AugmentData
        {
            augmentID = 4,
            displayName = "강력한 일격",
            description = "공격력이 20% 증가합니다 (중복: +10%)",
            rarity = AugmentRarity.Gold,
            targetStat = AugmentTargetStat.AttackDamage,
            modifierType = StatModifier.ModifierType.Multiplicative,
            firstValue = 0.2f,
            stackValue = 0.1f
        };

        dataDict[5] = new AugmentData
        {
            augmentID = 5,
            displayName = "신속한 공격",
            description = "공격속도가 30% 증가합니다 (중복: +15%)",
            rarity = AugmentRarity.Gold,
            targetStat = AugmentTargetStat.AttackSpeed,
            modifierType = StatModifier.ModifierType.Multiplicative,
            firstValue = 0.3f,
            stackValue = 0.15f
        };

        dataDict[6] = new AugmentData
        {
            augmentID = 6,
            displayName = "장거리 저격",
            description = "공격 사거리가 25% 증가합니다 (중복: +15%)",
            rarity = AugmentRarity.Gold,
            targetStat = AugmentTargetStat.AttackRange,
            modifierType = StatModifier.ModifierType.Multiplicative,
            firstValue = 0.25f,
            stackValue = 0.15f
        };

        // ===== Platinum 증강 (ID 7~9) =====
        dataDict[7] = new AugmentData
        {
            augmentID = 7,
            displayName = "폭발 탄환",
            description = "총알 적중 시 5m 범위에 50% 피해를 줍니다",
            rarity = AugmentRarity.Platinum,
            eventType = AugmentEventType.OnHit_AOE,
            eventValue1 = 5f,    // 범위
            eventValue2 = 0.5f   // 데미지 비율
        };

        dataDict[8] = new AugmentData
        {
            augmentID = 8,
            displayName = "연발 사격",
            description = "공격 시 총알을 2발 발사합니다",
            rarity = AugmentRarity.Platinum,
            eventType = AugmentEventType.OnAttack_DoubleBullet
        };

        dataDict[9] = new AugmentData
        {
            augmentID = 9,
            displayName = "약화의 저주",
            description = "적중한 적의 공격력을 10% 감소시킵니다",
            rarity = AugmentRarity.Platinum,
            eventType = AugmentEventType.OnHit_Debuff,
            eventValue1 = 0.1f   // 감소 비율
        };

        LogHelper.Log("✅ AugmentData loaded: 9 augments");
    }

    void GroupByRarity()
    {
        rarityDict[AugmentRarity.Silver] = dataDict.Values.Where(a => a.rarity == AugmentRarity.Silver).ToList();
        rarityDict[AugmentRarity.Gold] = dataDict.Values.Where(a => a.rarity == AugmentRarity.Gold).ToList();
        rarityDict[AugmentRarity.Platinum] = dataDict.Values.Where(a => a.rarity == AugmentRarity.Platinum).ToList();
    }

    public AugmentData GetData(int augmentID)
    {
        return dataDict.TryGetValue(augmentID, out var data) ? data : null;
    }

    public List<AugmentData> GetByRarity(AugmentRarity rarity)
    {
        return rarityDict.TryGetValue(rarity, out var list) ? list : new List<AugmentData>();
    }
}
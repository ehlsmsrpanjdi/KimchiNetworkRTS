using System;

public enum AugmentRarity
{
    Silver,
    Gold,
    Platinum
}

public enum AugmentTargetStat
{
    None,
    AttackDamage,
    AttackSpeed,
    AttackRange
}

public enum AugmentEventType
{
    None,
    OnHit_AOE,              // 적중 시 AOE 데미지
    OnAttack_DoubleBullet,  // 공격 시 총알 2발
    OnHit_Debuff            // 적중 시 타겟 공격력 감소
}

[Serializable]
public class AugmentData
{
    // 기본 정보
    public int augmentID;
    public string displayName;
    public string description;
    public AugmentRarity rarity;

    // Stat 증강용
    public AugmentTargetStat targetStat;
    public StatModifier.ModifierType modifierType;  // Additive or Multiplicative
    public float firstValue;   // 첫 획득 값
    public float stackValue;   // 중복 획득 값

    // Event 증강용
    public AugmentEventType eventType;
    public float eventValue1;
    public float eventValue2;
    public float eventValue3;

    public bool IsStatAugment => targetStat != AugmentTargetStat.None;
    public bool IsEventAugment => eventType != AugmentEventType.None;
}
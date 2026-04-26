using System;

// ========== 카드 레어도 (엑셀 Enums: Rarity) ==========
public enum CardRarity
{
    Common,
    Uncommon,
    Rare,
    Epic
}

// ========== 효과 타입 (엑셀 Enums: MutatorType) ==========
public enum CardEffectType
{
    StatAdd,        // 스탯 덧셈
    StatMul,        // 스탯 곱셈 (비율)
    StatSet,        // 스탯 고정값 설정
    AddBehavior     // 특수 행동 추가 (DoT, ChainLightning 등)
}

// ========== 연산자 (엑셀 Enums: Op) ==========
public enum CardOp
{
    Add,
    Mul,
    Set
}

// ================================================================
// 플레이어가 선택하는 카드 (엑셀 Cards_Building 시트)
// 웨이브 종료 후 3장 중 1장 선택 → 플레이어 소유 건물에 영구 적용
// ================================================================
[Serializable]
public class PlayerCardData
{
    public string cardID;           // ex: "CARD_ALL_DMG_C01"
    public string displayName;
    public CardRarity rarity;
    public int weight;              // 가중치 기반 뽑기용
    public string iconKey;
    public string description;

    // 효과 정의
    public CardEffectType effectType;
    public string statKey;          // "Damage", "FireRate", "Range", "SplashRadius", "FireCount" ...
    public CardOp op;
    public float value;

    // AddBehavior 전용 - Effects 시트 참조
    public string effectID;         // ex: "EFF_BURN_DOT"

    public bool IsBehaviorCard => effectType == CardEffectType.AddBehavior;
    public bool IsStatCard => effectType != CardEffectType.AddBehavior;
}

// ================================================================
// 몬스터에게 자동 적용되는 카드 (엑셀 Cards_Monster 시트)
// 플레이어가 선택하는 순간 서버가 자동으로 뽑아 다음 웨이브부터 누적 적용
// 플레이어는 선택하지 않고 존재조차 모를 수 있음
// ================================================================
[Serializable]
public class MonsterCardData
{
    public string cardID;           // ex: "CARDM_HP_C01"
    public string displayName;
    public CardRarity rarity;
    public int weight;
    public string iconKey;
    public string description;

    // 효과 정의
    public CardEffectType effectType;
    public string statKey;          // "MaxHP", "MoveSpeed", "Armor", "AttackDamage", "CountBase" ...
    public CardOp op;
    public float value;

    // AddBehavior 전용
    public string effectID;

    public bool IsBehaviorCard => effectType == CardEffectType.AddBehavior;
    public bool IsStatCard => effectType != CardEffectType.AddBehavior;
}

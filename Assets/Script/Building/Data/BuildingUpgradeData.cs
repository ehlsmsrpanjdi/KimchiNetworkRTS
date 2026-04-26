using System;

// ===== 업그레이드 뮤테이터 타입 (엑셀 Enums: MutatorType) =====
public enum UpgradeMutatorType
{
    StatAdd,        // 스탯 덧셈
    StatMul,        // 스탯 곱셈
    StatSet,        // 스탯 고정값
    AddBehavior,    // 특수 행동 추가
    AddTag          // 태그 추가
}

[Serializable]
public class BuildingUpgradeData
{
    // ===== 기본 정보 (엑셀 BuildingUpgrades 시트) =====
    public string upgradeID;        // ex: "UPG_TOWER_DMG"
    public string displayName;
    public string appliesToBuildingID;  // 어느 건물/그룹에 적용
    public int tier;                // 업그레이드 티어
    public ResourceType costResourceType;
    public int cost;
    public int maxStacks;           // 최대 중첩 가능 횟수
    public string prereqUpgradeID;  // 선행 업그레이드 ID (없으면 "")
    public string iconKey;
    public string description;

    // ===== 효과 =====
    public UpgradeMutatorType mutatorType;
    public string statKey;          // 적용할 스탯 키
    public CardOp op;               // Add / Mul / Set
    public float value;             // 적용 값
    public string exclusiveGroup;   // 상호 배타 그룹 (같은 그룹 내 1개만 선택)
}

using System;
using Unity.Netcode;
using UnityEngine;

// ========== 건물 분류 (엑셀 Enums: BuildingCategory) ==========
public enum BuildingCategory
{
    Core,       // 코어 (파괴 시 게임 오버)
    Wall,       // 방어벽
    Tower,      // 공격 타워
    Trap,       // 함정 (적 접촉 시 발동)
    Support,    // 지원 건물
    Resource,   // 자원 생산
    Utility     // 특수 유틸 (거래소 등)
    // 추후 추가 예정: Barracks, Artillery, Defense 등
}

// ========== 데미지 타입 (엑셀 Enums: DamageType) ==========
public enum DamageType
{
    None,
    Physical,
    Fire,
    Ice,
    Electric,
    Poison
}

// ========== 타겟팅 규칙 (엑셀 Enums: TargetingRule) ==========
public enum TargetingRule
{
    Nearest,
    FirstInRange,
    LowestHP,
    HighestHP,
    Strongest
}

// ========== 건설 비용 ==========
[Serializable]
public struct ResourceCost : INetworkSerializable
{
    public ResourceType resourceType;
    public int amount;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref resourceType);
        serializer.SerializeValue(ref amount);
    }
}

// ========== 건물 데이터 (엑셀 Buildings 시트) ==========
[Serializable]
public class BuildingData
{
    // ===== 기본 정보 =====
    public string buildingID;       // 엑셀 BuildingId (ex: "TOWER_BASIC")
    public string displayName;
    public BuildingCategory category;
    public string description;
    public string iconKey;
    public string prefabKey;        // 엑셀 PrefabKey
    public string bulletPrefabKey;  // 엑셀 BulletPrefabKey

    // ===== 건물 크기 =====
    public int sizeX;
    public int sizeY;

    // ===== 건설 비용 =====
    public ResourceCost[] constructionCosts;

    // ===== 기본 스탯 =====
    public float baseMaxHP;
    public float baseArmor;         // 엑셀 Armor (방어력)

    // ===== 공격 스탯 (Tower/Trap 전용) =====
    public bool canAttack;          // 엑셀 CanAttack
    public float baseDamage;        // Damage (Tower: 피해 / Resource: 최대 저장량 / Wall: 초당 회복량)
    public DamageType damageType;
    public float baseRange;         // Range
    public float baseFireRate;      // FireRate = 공격 간격(초). 값이 작을수록 빠름
    public int baseFireCount;       // FireCount = 연사 탄환 수
    public TargetingRule targetingRule;
    public float splashRadius;      // SplashRadius
    public float bulletSpeed;

    // ===== 자원 타워 전용 =====
    public ResourceType resourceGenType;    // 생산 자원 종류 (Wood / Metal)
    public bool tradeEnabled;              // 거래소 여부

    // ===== 편의 프로퍼티 =====
    public bool IsAttackBuilding => canAttack;
    public bool IsResourceBuilding => category == BuildingCategory.Resource;
    public bool IsWallBuilding => category == BuildingCategory.Wall;
    public bool IsCoreBuilding => category == BuildingCategory.Core;
    public bool IsTrapBuilding => category == BuildingCategory.Trap;
}

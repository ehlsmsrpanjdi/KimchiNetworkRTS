using System;
using Unity.Netcode;
using UnityEngine;

// ========== 건물 분류 ==========
public enum BuildingCategory
{
    Attack,    // 공격
    Resource,  // 자원
    Support,   // 지원
    Wall       // 방어벽
}

// ========== 자원 종류 ==========
public enum ResourceType
{
    Wood,
    Iron,
}

// ========== 공격 타입 ==========
public enum AttackType
{
    Single,    // 단일 대상
    AOE,       // 광역
    Splash     // 스플래시
}

// ========== 공격 우선순위 ==========
public enum AttackPriority
{
    Nearest,   // 가장 가까운 적
    LowestHP,  // 체력 가장 낮은 적
    Strongest  // 가장 강한 적
}

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

[System.Serializable]
public class BuildingData
{
    // ===== 기본 정보 =====
    public int buildingID;
    public string displayName;
    public BuildingCategory category;
    public string description;
    public string iconKey;

    // ===== 건물 크기 =====
    public int sizeX;
    public int sizeY;

    // ===== 건설 비용 (여러 자원) =====
    public ResourceCost[] constructionCosts;

    // ===== 기본 스탯 =====
    public float baseMaxHP;
    public float baseDefense;

    // ===== 공통 스탯 (용도가 건물 타입마다 다름) =====
    public bool isAttackTower;
    public float baseAttackDamage;  // 공격: 데미지 / 자원: 최대 스택
    public AttackType attackType;
    public float baseAttackRange;   // 공격: 사거리 / 벽: 수리 범위 / 자원: 수확 범위
    public float baseAttackSpeed;   // 공격: 속도 / 자원: 초당 획득 / 벽: 초당 수리

    public AttackPriority attackPriority;
    public float aoeDamageRadius;

    // ===== 탄환 관련 =====
    public int bulletPrefabID;
    public float bulletSpeed;
    public int bulletMovementID;

    // ===== 자원 타워 전용 =====
    public ResourceType resourceType;  // 생산할 자원 종류
    public float harvestDuration;      // 수확 소요 시간
}
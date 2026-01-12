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

    // ===== 공격 관련 =====
    public bool isAttackTower;
    public float baseAttackDamage;
    public AttackType attackType;
    public float baseAttackRange;
    public float baseAttackSpeed;
    public AttackPriority attackPriority;
    public float aoeDamageRadius;

    // ===== 탄환 관련 =====
    public int bulletPrefabID;
    public float bulletSpeed;         // ✅ 추가
    public int bulletMovementID;      // ✅ 추가 (1=직선, 2=포물선, 3=호밍)

    // ===== 자원 생산 =====
    public ResourceType resourceType;
    public float baseResourceRate;
}
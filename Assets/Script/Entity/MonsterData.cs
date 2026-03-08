using System;
using UnityEngine;

// ========== 몬스터 분류 (엑셀 Enums: EnemyArchetype) ==========
public enum EnemyArchetype
{
    Melee,
    Ranged,
    Tank,
    Flying,
    Support,
    Boss
}

[Serializable]
public class MonsterData
{
    // ===== 기본 정보 (엑셀 Enemies 시트) =====
    public string monsterID;        // 엑셀 EnemyId (ex: "ENEMY_GRUNT_01")
    public string displayName;
    public EnemyArchetype archetype;
    public string description;
    public string iconKey;
    public string prefabKey;        // 엑셀 PrefabKey

    // ===== 기본 스탯 =====
    public float baseMaxHP;
    public float baseArmor;         // 방어력 (피해 감소)
    public float baseMoveSpeed;
    public float baseAttackDamage;
    public float baseAttackRange;
    public float baseAttackRate;    // 공격 간격(초) - 작을수록 빠름

    // ===== 보상 =====
    public ResourceType rewardResourceType; // Reward_ResourceId
    public int rewardAmount;

    // ===== 시간 비례 강화 (엑셀 StatRate) =====
    // StatRate: 웨이브마다 스탯을 몇 % 증가시킬지
    // ex) 0.06 → 웨이브당 6% 증가
    public float statRate;

    // ===== 편의 프로퍼티 =====
    public bool IsBoss => archetype == EnemyArchetype.Boss;
    public bool IsFlying => archetype == EnemyArchetype.Flying;
    public bool IsSupport => archetype == EnemyArchetype.Support;
}

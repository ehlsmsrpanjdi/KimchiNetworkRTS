using UnityEngine;
using System;

public enum MonsterType
{
    Melee,      // 근접
    Ranged,     // 원거리
    Boss        // 보스
}


[Serializable]
public class MonsterData
{
    // ===== 기본 정보 =====
    public int monsterID;               // 적 고유 ID
    public string displayName;          // 게임 내 표시 이름
    public MonsterType monsterType;     // 적 타입 분류
    public string description;          // 적 설명 텍스트
    public string iconKey;              // 도감, 표시용 아이콘
    public string prefabName;           // 프리팹 이름

    // ===== 기본 스탯 =====
    public float baseMaxHP;             // 적 최대체력
    public float baseDefense;           // 적 방어력
    public float baseMoveSpeed;         // 이동 속도
    public float baseAttackDamage;      // 공격 1회당 피해량
    public float baseAttackRange;       // 공격 사거리 (근접은 2 고정)
    public float baseAttackSpeed;       // 공격 속도 (초당 공격 횟수)

    // ===== 시간 별 강화 =====
    [Tooltip("n분마다 스탯 강화 (0 = 강화 없음)")]
    public float scalingInterval = 0f;  // 강화 주기 (분 단위)

    [Tooltip("강화 주기마다 증가하는 비율 (0.1 = 10% 증가)")]
    public float hpScaling = 0.1f;      // 체력 증가율
    public float damageScaling = 0.1f;  // 공격력 증가율
    public float defenseScaling = 0.05f; // 방어력 증가율

    // ===== 보스 전용 =====
    [Tooltip("보스 여부 (true면 웨이브당 1마리 제한)")]
    public bool isBoss = false;
}
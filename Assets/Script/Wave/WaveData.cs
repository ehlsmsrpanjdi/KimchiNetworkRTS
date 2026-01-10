using UnityEngine;
using System;

[Serializable]
public class WaveSpawnInfo
{
    public int monsterID;           // 스폰될 몬스터 ID
    public int baseSpawnCount;      // 기본 스폰 수
    public int perPlayerSpawnCount; // 플레이어당 추가 스폰 수
}

[Serializable]
public class WaveData
{
    // ===== 웨이브 정보 =====
    public int waveID;              // 웨이브 고유 ID
    public int waveNumber;          // 표시용 웨이브 번호 (1웨이브, 2웨이브...)

    // ===== 스폰 정보 =====
    public WaveSpawnInfo[] spawnInfos; // 이 웨이브에서 스폰될 몬스터들

    // ===== 스폰 타이밍 =====
    [Tooltip("웨이브 진행 시간 (초)")]
    public float waveDuration = 60f;

    // ===== 보스 정보 =====
    [Tooltip("보스 몬스터 ID (-1이면 보스 없음)")]
    public int bossMonsterID = -1;

    // ===== 계산 메서드 =====
    /// <summary>
    /// 특정 몬스터의 총 스폰 수 계산
    /// </summary>
    public int GetTotalSpawnCount(int monsterID, int playerCount)
    {
        foreach (var info in spawnInfos)
        {
            if (info.monsterID == monsterID)
            {
                return info.baseSpawnCount + (info.perPlayerSpawnCount * playerCount);
            }
        }
        return 0;
    }

    /// <summary>
    /// 특정 몬스터의 스폰 간격 계산
    /// 예: 10마리, 60초 → 6초마다 1마리
    /// </summary>
    public float GetSpawnInterval(int monsterID, int playerCount)
    {
        int totalCount = GetTotalSpawnCount(monsterID, playerCount);
        if (totalCount <= 0) return 0f;

        return waveDuration / totalCount;
    }

    /// <summary>
    /// 이 웨이브에 보스가 있는지
    /// </summary>
    public bool HasBoss()
    {
        return bossMonsterID >= 0;
    }
}
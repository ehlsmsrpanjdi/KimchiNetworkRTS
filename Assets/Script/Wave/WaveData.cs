using System;
using UnityEngine;

// ===== 웨이브 내 스폰 정보 (엑셀 Waves 시트 1행 = 1 WaveSpawnInfo) =====
[Serializable]
public class WaveSpawnInfo
{
    public string monsterID;            // 엑셀 EnemyId (ex: "ENEMY_GRUNT_01")
    public int countBase;               // 기본 스폰 수
    public int countPerPlayer;          // 플레이어당 추가 스폰 수
    public float spawnDurationSec;      // 스폰 지속 시간(초)
    public float spawnIntervalSec;      // 스폰 간격(초) - 엑셀에서 직접 지정
}

// ===== 웨이브 데이터 (동일 waveNumber를 가진 여러 행을 하나로 묶음) =====
[Serializable]
public class WaveData
{
    public int waveNumber;
    public WaveSpawnInfo[] spawnInfos;

    // 보스 웨이브 여부 (IsBossWave = TRUE인 행이 있으면 true)
    public bool isBossWave;

    // 이 웨이브 종료 후 제시할 카드 선택지 수
    public int cardChoices;             // 엑셀 CardMonsterChoices

    // ========== 계산 메서드 ==========

    /// <summary>
    /// 특정 monsterID의 총 스폰 수
    /// </summary>
    public int GetTotalCount(string monsterID, int playerCount)
    {
        foreach (var info in spawnInfos)
        {
            if (info.monsterID == monsterID)
                return info.countBase + info.countPerPlayer * playerCount;
        }
        return 0;
    }

    /// <summary>
    /// 엑셀에 직접 정의된 스폰 간격 반환
    /// (기존: waveDuration/totalCount로 계산 → 변경: 엑셀 값 직접 사용)
    /// </summary>
    public float GetSpawnInterval(string monsterID)
    {
        foreach (var info in spawnInfos)
        {
            if (info.monsterID == monsterID)
                return info.spawnIntervalSec;
        }
        return 1f;
    }
}

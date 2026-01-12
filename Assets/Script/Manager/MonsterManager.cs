using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class MonsterManager : NetworkBehaviour
{
    public static MonsterManager Instance;

    // ✅ 모든 살아있는 몬스터 리스트
    private List<MonsterBase> aliveMonsters = new List<MonsterBase>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========== 몬스터 등록/해제 ==========

    /// <summary>
    /// 몬스터 스폰 시 호출
    /// </summary>
    public void RegisterMonster(MonsterBase monster)
    {
        if (!IsServer) return;

        if (!aliveMonsters.Contains(monster))
        {
            aliveMonsters.Add(monster);
            LogHelper.Log($"✅ Monster registered: {monster.data?.displayName ?? "Unknown"} (Total: {aliveMonsters.Count})");
        }
    }

    /// <summary>
    /// 몬스터 사망 시 호출
    /// </summary>
    public void UnregisterMonster(MonsterBase monster)
    {
        if (!IsServer) return;

        if (aliveMonsters.Remove(monster))
        {
            LogHelper.Log($"💀 Monster unregistered: {monster.data?.displayName ?? "Unknown"} (Remaining: {aliveMonsters.Count})");
        }
    }

    // ========== 몬스터 조회 ==========

    /// <summary>
    /// 살아있는 몬스터 수
    /// </summary>
    public int GetAliveMonsterCount()
    {
        return aliveMonsters.Count;
    }

    /// <summary>
    /// 살아있는 몬스터 리스트
    /// </summary>
    public List<MonsterBase> GetAliveMonsters()
    {
        return aliveMonsters;
    }

    /// <summary>
    /// 모든 몬스터가 죽었는지
    /// </summary>
    public bool AreAllMonstersDead()
    {
        // ✅ MonsterManager 사용 (빠름!)
        return MonsterManager.Instance.AreAllMonstersDead();
    }

    /// <summary>
    /// 주기적으로 null 정리 (선택사항)
    /// </summary>
    public void CleanupDestroyedMonsters()
    {
        aliveMonsters.RemoveAll(m => m == null || !m.gameObject.activeSelf);
    }
}
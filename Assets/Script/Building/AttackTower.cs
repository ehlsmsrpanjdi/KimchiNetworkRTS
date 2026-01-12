using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 공격 건물 (총알 발사)
/// </summary>
public class AttackTower : BuildingBase
{
    [Header("Attack")]
    private float lastAttackTime;
    private MonsterBase currentTarget;
    private bool isInitialized = false;  // ✅ 이 줄 추가

    protected override void Update()
    {
        base.Update();

        if (!IsServer) return;
        if (!isInitialized) return;  // ✅ 변경됨

        UpdateTarget();
        TryAttack();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (data != null && data.isAttackTower)
        {
            isInitialized = true;
            LogHelper.Log($"✅ AttackTower initialized: {data.displayName}");
        }
    }


    // ========== 타겟 찾기 ==========
    void UpdateTarget()
    {
        // 현재 타겟이 유효한지 체크
        if (IsValidTarget(currentTarget))
        {
            return;
        }

        // 새 타겟 찾기
        currentTarget = FindTarget();
    }

    bool IsValidTarget(MonsterBase target)
    {
        if (target == null || !target.gameObject.activeSelf)
            return false;

        // 사거리 체크
        float distance = Vector3.Distance(transform.position, target.transform.position);
        return distance <= stat.attackRange.Value;
    }

    MonsterBase FindTarget()
    {
        List<MonsterBase> monsters = MonsterManager.Instance.GetAliveMonsters();

        MonsterBase bestTarget = null;
        float bestValue = Mathf.Infinity;

        foreach (var monster in monsters)
        {
            if (monster == null || !monster.gameObject.activeSelf)
                continue;

            float distance = Vector3.Distance(transform.position, monster.transform.position);

            // 사거리 밖이면 스킵
            if (distance > stat.attackRange.Value)
                continue;

            // 우선순위에 따라 타겟 선택
            float value = GetTargetPriority(monster, distance);

            if (value < bestValue)
            {
                bestValue = value;
                bestTarget = monster;
            }
        }

        return bestTarget;
    }

    float GetTargetPriority(MonsterBase monster, float distance)
    {
        return data.attackPriority switch
        {
            AttackPriority.Nearest => distance,
            AttackPriority.LowestHP => monster.currentHP.Value,
            AttackPriority.Strongest => -monster.maxHP.Value, // 음수로 해서 큰 값이 우선
            _ => distance
        };
    }

    // ========== 공격 ==========
    void TryAttack()
    {
        if (currentTarget == null)
            return;

        // 공격 쿨다운 체크
        float attackCooldown = 1f / stat.attackSpeed.Value;
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        // 공격 실행
        PerformAttack();
        lastAttackTime = Time.time;
    }

    void PerformAttack()
    {
        // OnAttack 이벤트 발동 (Modifier용)
        TriggerOnAttack();

        // 총알 발사
        FireBullet(currentTarget);

        LogHelper.Log($"🔫 {data.displayName} attacked {currentTarget.data?.displayName}");
    }

    // ========== 총알 발사 ==========
    void FireBullet(MonsterBase target)
    {
        string bulletPrefabName = GetBulletPrefabName(data.bulletPrefabID);
        GameObject bulletPrefab = AssetManager.Instance.GetByName(bulletPrefabName);

        if (bulletPrefab == null)
        {
            LogHelper.LogError($"Bullet prefab not found: {bulletPrefabName}");
            return;
        }

        Vector3 spawnPos = transform.position + Vector3.up * 1f;

        // ✅ 풀에서 가져오기
        NetworkObject netObj = NetworkPoolManager.Instance.GetFromPool(
            bulletPrefab,
            spawnPos,
            Quaternion.identity
        );

        var bullet = netObj.GetComponent<BulletBase>();

        if (bullet == null)
        {
            LogHelper.LogError($"BulletBase missing on {bulletPrefabName}");
            return;
        }

        // 총알 초기화
        float finalDamage = stat.GetFinalAttackDamage(modifierManager);
        bullet.Initialize(
            this,
            target,
            finalDamage,
            data.bulletMovementID,
            data.bulletSpeed
        );

        // Spawn
        netObj.Spawn();
    }

    string GetBulletPrefabName(int bulletID)
    {
        return bulletID switch
        {
            1 => "BasicBullet",
            2 => "MagicBullet",
            3 => "RocketBullet",
            _ => "BasicBullet"
        };
    }

    // ========== 디버그 ==========
    void OnDrawGizmosSelected()
    {
        if (data == null) return;

        // 공격 사거리 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stat.attackRange.Value);

        // 타겟 라인
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 공격 건물 (총알 발사)
/// 플레이어 근처: 공격력/공격속도 1.5배
/// </summary>
public class AttackTower : BuildingBase
{
    [Header("Attack")]
    private float lastAttackTime;
    private MonsterBase currentTarget;
    private bool isInitialized = false;

    [Header("Player Boost")]
    private bool isBoosted = false;
    private float baseDamage;
    private float baseSpeed;

    public MonsterBase CurrentTarget => currentTarget;

    protected override void Update()
    {
        base.Update();

        if (!IsServer) return;
        if (!isInitialized) return;

        UpdateTarget();
        TryAttack();
    }

    // ========== 플레이어 근접 효과 ==========
    protected override void OnPlayerEnterRange(Player player)
    {
        if (!isBoosted)
        {
            // 원본 값 저장
            baseDamage = stat.attackDamage.Value;
            baseSpeed = stat.attackSpeed.Value;

            // 1.5배 버프
            stat.attackDamage.Value *= 1.5f;
            stat.attackSpeed.Value *= 1.5f;

            isBoosted = true;
            LogHelper.Log($"⚡ Attack boost activated! Damage: {stat.attackDamage.Value}, Speed: {stat.attackSpeed.Value}");
        }
    }

    protected override void OnPlayerExitRange(Player player)
    {
        if (isBoosted)
        {
            // 원본 값 복구
            stat.attackDamage.Value = baseDamage;
            stat.attackSpeed.Value = baseSpeed;

            isBoosted = false;
            LogHelper.Log($"⚡ Attack boost deactivated! Damage: {stat.attackDamage.Value}, Speed: {stat.attackSpeed.Value}");
        }
    }

    // ========== 타겟 찾기 ==========
    void UpdateTarget()
    {
        if (IsValidTarget(currentTarget))
        {
            return;
        }

        currentTarget = FindTarget();
    }

    bool IsValidTarget(MonsterBase target)
    {
        if (target == null || !target.gameObject.activeSelf)
            return false;

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

            if (distance > stat.attackRange.Value)
                continue;

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
            AttackPriority.Strongest => -monster.maxHP.Value,
            _ => distance
        };
    }

    // ========== 공격 ==========
    void TryAttack()
    {
        if (currentTarget == null)
            return;

        float attackCooldown = 1f / stat.attackSpeed.Value;
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        PerformAttack();
        lastAttackTime = Time.time;
    }

    void PerformAttack()
    {
        TriggerOnAttack();
        StartCoroutine(FireBulletsWithDelay());
    }

    IEnumerator FireBulletsWithDelay()
    {
        for (int i = 0; i < stat.bulletCountPerAttack.Value; i++)
        {
            FireBullet(currentTarget);

            if (i < stat.bulletCountPerAttack.Value - 1)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void FireBullet(MonsterBase target)
    {
        string bulletPrefabName = GetBulletPrefabName(data.bulletPrefabID);
        GameObject bulletPrefab = AssetManager.Instance.GetByName(bulletPrefabName);

        if (bulletPrefab == null)
        {
            LogHelper.LogError($"Bullet prefab not found: {bulletPrefabName}");
            return;
        }

        Vector3 spawnPos = transform.position + Vector3.up * 1f;

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

        float finalDamage = stat.GetFinalAttackDamage(modifierManager);
        bullet.Initialize(
            this,
            target,
            finalDamage,
            data.bulletMovementID,
            data.bulletSpeed
        );

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

    // ========== 초기화 ==========
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (data != null && data.isAttackTower)
        {
            isInitialized = true;
            LogHelper.Log($"✅ AttackTower initialized: {data.displayName}");
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnPop()
    {
        base.OnPop();
        isInitialized = false;
        isBoosted = false;
    }

    public override void OnPush()
    {
        base.OnPush();
        isInitialized = false;
        currentTarget = null;
        lastAttackTime = 0f;
        isBoosted = false;
    }

    // ========== 디버그 ==========
    void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stat.attackRange.Value);

        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MonsterBase : NetworkBehaviour, ITakeDamage, IPoolObj
{
    [Header("Monster Identity")]
    public int monsterID;
    private MonsterData data;

    [Header("Components")]
    public NavMeshAgent agent;
    private Animator animator;

    [Header("Stats")]
    public NetworkVariable<float> currentHP = new NetworkVariable<float>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<float> maxHP = new NetworkVariable<float>();
    public NetworkVariable<float> defense = new NetworkVariable<float>();
    public NetworkVariable<float> attackDamage = new NetworkVariable<float>();
    public NetworkVariable<float> attackSpeed = new NetworkVariable<float>();
    public NetworkVariable<float> attackRange = new NetworkVariable<float>();
    public NetworkVariable<float> moveSpeed = new NetworkVariable<float>();

    [Header("Combat")]
    private GameObject currentTarget;
    private float lastAttackTime;

    [Header("Spawned Time")]
    private float spawnedTime; // 스폰된 시간 (게임 시간 기준)

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // ========== 초기화 ==========
    public void Initialize(int id, float gameTime)
    {
        if (!IsServer) return;

        monsterID = id;
        spawnedTime = gameTime;

        data = MonsterDataManager.Instance.GetData(id);
        if (data == null)
        {
            LogHelper.LogError($"MonsterData not found: {id}");
            return;
        }

        // 시간에 따른 스탯 스케일링 계산
        float scalingMultiplier = CalculateScalingMultiplier(gameTime);

        // 스탯 초기화 (스케일링 적용)
        maxHP.Value = data.baseMaxHP * (1f + data.hpScaling * scalingMultiplier);
        currentHP.Value = maxHP.Value;
        defense.Value = data.baseDefense * (1f + data.defenseScaling * scalingMultiplier);
        attackDamage.Value = data.baseAttackDamage * (1f + data.damageScaling * scalingMultiplier);
        attackSpeed.Value = data.baseAttackSpeed;
        attackRange.Value = data.baseAttackRange;
        moveSpeed.Value = data.baseMoveSpeed;

        // NavMeshAgent 설정
        if (agent != null)
        {
            agent.speed = moveSpeed.Value;
        }

        LogHelper.Log($"✅ Monster initialized: {data.displayName} (HP: {maxHP.Value}, DMG: {attackDamage.Value})");
    }

    // ========== 시간 스케일링 계산 ==========
    float CalculateScalingMultiplier(float gameTime)
    {
        if (data.scalingInterval <= 0f) return 0f;

        // gameTime(초)을 분 단위로 변환
        float gameTimeMinutes = gameTime / 60f;

        // 강화 횟수 계산
        return Mathf.Floor(gameTimeMinutes / data.scalingInterval);
    }

    // ========== AI 업데이트 ==========
    void Update()
    {
        if (!IsServer) return;

        UpdateTarget();
        UpdateMovement();
        UpdateAttack();
    }

    void UpdateTarget()
    {
        // 현재 타겟이 유효한지 체크
        if (currentTarget != null && currentTarget.activeSelf)
        {
            return;
        }

        // 새 타겟 찾기
        currentTarget = FindNearestTarget();
    }

    GameObject FindNearestTarget()
    {
        // 1) 먼저 플레이어 찾기
        GameObject nearestPlayer = FindNearestPlayer();

        // 2) 플레이어가 있고 접근 가능하면 플레이어 타겟
        if (nearestPlayer != null && IsPlayerReachable(nearestPlayer))
        {
            return nearestPlayer;
        }

        // 3) 플레이어가 없거나 접근 불가면 건물 타겟
        return FindNearestBuilding();
    }

    GameObject FindNearestPlayer()
    {
        List<Player> players = PlayerManager.Instance.GetAlivePlayers();

        GameObject nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var player in players)
        {
            if (player == null || !player.gameObject.activeSelf) continue;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = player.gameObject;
            }
        }

        return nearest;
    }

    GameObject FindNearestBuilding()
    {
        List<BuildingBase> buildings = BuildingManager.Instance.GetAliveBuildings();

        GameObject nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var building in buildings)
        {
            if (building == null || !building.gameObject.activeSelf) continue;

            float distance = Vector3.Distance(transform.position, building.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = building.gameObject;
            }
        }

        return nearest;
    }

    bool IsPlayerReachable(GameObject player)
    {
        // TODO: NavMesh 경로 체크
        // 지금은 간단하게 true
        return true;
    }
    void UpdateMovement()
    {
        if (currentTarget == null || agent == null) return;

        // 공격 사거리 밖이면 이동
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distance > attackRange.Value)
        {
            agent.SetDestination(currentTarget.transform.position);
            agent.isStopped = false;
        }
        else
        {
            agent.isStopped = true;
        }
    }

    void UpdateAttack()
    {
        if (currentTarget == null) return;

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

        // 공격 사거리 안이면 공격
        if (distance <= attackRange.Value)
        {
            // 공격 속도 체크 (초당 공격 횟수)
            float attackCooldown = 1f / attackSpeed.Value;
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    void PerformAttack()
    {
        if (currentTarget == null) return;

        var damageable = currentTarget.GetComponent<ITakeDamage>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage.Value);
            LogHelper.Log($"{data.displayName} attacked for {attackDamage.Value} damage");
        }
    }

    // ========== ITakeDamage 구현 ==========
    public void TakeDamage(float damage)
    {
        if (!IsServer) return;

        float finalDamage = Mathf.Max(0, damage - defense.Value);
        currentHP.Value -= finalDamage;

        LogHelper.Log($"{data.displayName} took {finalDamage} damage (HP: {currentHP.Value}/{maxHP.Value})");

        if (currentHP.Value <= 0f)
        {
            OnDeath();
        }
    }

    void OnDeath()
    {
        if (!IsServer) return;

        LogHelper.Log($"💀 {data.displayName} died!");

        // TODO: 자원 드롭
        // TODO: 경험치 지급

        // Despawn (Handler가 풀 반환)
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }
    }

    // ========== IPoolObj 구현 ==========
    public void OnPop()
    {
        gameObject.SetActive(true);

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        // ✅ 변수 초기화 (데이터는 Initialize에서)
        currentTarget = null;
        lastAttackTime = 0f;
    }

    public void OnPush()
    {
        gameObject.SetActive(false);

        currentTarget = null;
        lastAttackTime = 0f;

        if (agent != null)
        {
            agent.enabled = false;
        }
    }

    // ========== 디버그 ==========
    void OnDrawGizmosSelected()
    {
        if (data == null) return;

        // 공격 사거리 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange.Value);

        // 타겟 라인
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}
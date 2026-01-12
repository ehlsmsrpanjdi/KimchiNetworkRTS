using Unity.Netcode;
using UnityEngine;

public class BulletBase : NetworkBehaviour, IPoolObj
{
    [Header("Bullet Info")]
    public int bulletID;
    public float damage;
    public float moveSpeed;

    [Header("Target")]
    public MonsterBase targetMonster;      // 타겟 몬스터 (죽을 수 있음)
    public Vector3 targetPosition;         // 발사 시점의 타겟 위치 (고정)

    [Header("Movement")]
    private IBulletMovement movementStrategy;

    [Header("Owner")]
    public BuildingBase ownerBuilding;     // OnHit 이벤트용

    [Header("Journey Data (Parabolic용)")]
    public float journeyStartTime = -1f;
    public Vector3 journeyStartPosition;
    public float journeyDistance;
    public float journeyDuration;

    // ========== 초기화 ==========
    public void Initialize(BuildingBase owner, MonsterBase target, float dmg, int movementID, float speed)
    {
        ownerBuilding = owner;
        targetMonster = target;
        targetPosition = target.transform.position; // 발사 시점 위치 저장
        damage = dmg;
        moveSpeed = speed;

        // Journey 데이터 초기화
        journeyStartTime = -1f;

        // Movement 전략 설정
        movementStrategy = BulletMovementManager.Instance.GetMovement(movementID);

        LogHelper.Log($"🔵 Bullet initialized: {owner.buildingID} -> {target.data?.displayName}, Movement: {movementID}");
    }

    // ========== Update ==========
    void Update()
    {
        if (!IsServer) return;

        // Movement 실행
        movementStrategy?.Move(this, Time.deltaTime);

        // 도착 체크
        if (movementStrategy != null && movementStrategy.HasReachedTarget(this))
        {
            OnReachTarget();
        }
    }

    // ========== 타겟 도착 ==========
    void OnReachTarget()
    {
        // 타겟이 죽었으면 그냥 소멸
        if (targetMonster == null || !targetMonster.gameObject.activeSelf)
        {
            LogHelper.Log("💨 타겟 사망, 총알 소멸");
            ReturnToPool();
            return;
        }

        // 데미지 적용
        targetMonster.TakeDamage(damage);
        LogHelper.Log($"💥 Bullet hit: {damage} damage to {targetMonster.data?.displayName}");

        // ✅ Owner Building의 OnHit 이벤트 발동 (Modifier용)
        ownerBuilding?.TriggerOnHit();

        // TODO: 히트 이펙트

        // 풀 반환
        ReturnToPool();
    }

    void ReturnToPool()
    {
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
    }

    public void OnPush()
    {
        gameObject.SetActive(false);
        ownerBuilding = null;
        targetMonster = null;
        journeyStartTime = -1f;
    }
}
using Unity.Netcode;
using UnityEngine;

public class BulletBase : NetworkBehaviour, IPoolObj
{
    public float damage;
    public float moveSpeed;

    private MonsterBase targetMonster;
    private Vector3 lastKnownPosition;
    private BuildingBase ownerBuilding;

    public void Initialize(BuildingBase owner, MonsterBase target, float dmg, float speed)
    {
        ownerBuilding = owner;
        targetMonster = target;
        lastKnownPosition = target.transform.position;
        damage = dmg;
        moveSpeed = speed;
    }

    void Update()
    {
        if (!IsServer) return;

        Vector3 targetPos = (targetMonster != null && targetMonster.gameObject.activeSelf)
            ? targetMonster.transform.position
            : lastKnownPosition;

        if (targetMonster != null && targetMonster.gameObject.activeSelf)
            lastKnownPosition = targetPos;

        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
            OnReachTarget();
    }

    void OnReachTarget()
    {
        if (targetMonster != null && targetMonster.gameObject.activeSelf)
        {
            targetMonster.TakeDamage(damage);
            ownerBuilding?.TriggerOnHit(targetMonster);
        }

        ReturnToPool();
    }

    void ReturnToPool()
    {
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
            netObj.Despawn();
    }

    public void OnPop() => gameObject.SetActive(true);

    public void OnPush()
    {
        gameObject.SetActive(false);
        ownerBuilding = null;
        targetMonster = null;
    }
}

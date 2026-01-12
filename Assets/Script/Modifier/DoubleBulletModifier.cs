using System;
using UnityEngine;

/// <summary>
/// 공격 시 총알 2발 발사
/// </summary>
public class DoubleBulletModifier : EventModifier
{
    private AttackTower ownerTower;

    public DoubleBulletModifier(string id, string name) : base(id, name)
    {
    }

    public override void BindEvents(object owner)
    {
        if (owner is AttackTower tower)
        {
            ownerTower = tower;
            tower.OnAttack += OnAttackEvent;
        }
    }

    public override void UnbindEvents(object owner)
    {
        if (owner is AttackTower tower)
        {
            tower.OnAttack -= OnAttackEvent;
        }
    }

    void OnAttackEvent()
    {
        if (ownerTower == null) return;

        MonsterBase target = ownerTower.CurrentTarget;
        if (target == null || !target.gameObject.activeSelf)
        {
            return;
        }

        // 0.1초 후 두 번째 총알
        ownerTower.StartCoroutine(FireDelayedBullet(target, 0.1f));

        TriggerAndConsume(null);
    }

    System.Collections.IEnumerator FireDelayedBullet(MonsterBase target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target != null && target.gameObject.activeSelf)
        {
            ownerTower.FireBullet(target);
            LogHelper.Log($"🔫🔫 Second bullet fired!");
        }
    }
}
using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 총알 적중 시 주변에 AOE 데미지
/// </summary>
public class AOEModifier : EventModifier
{
    private float aoeRadius;
    private float damageRatio;
    private BuildingBase ownerBuilding;

    public AOEModifier(string id, string name, float radius, float ratio) : base(id, name)
    {
        aoeRadius = radius;
        damageRatio = ratio;
    }

    public override void BindEvents(object owner)
    {
        if (owner is BuildingBase building)
        {
            ownerBuilding = building;
            building.OnHit += OnHitEvent;
        }
    }

    public override void UnbindEvents(object owner)
    {
        if (owner is BuildingBase building)
        {
            building.OnHit -= OnHitEvent;
        }
    }

    void OnHitEvent(MonsterBase hitTarget)  // ✅ 매개변수 추가
    {
        if (ownerBuilding == null) return;

        // hitTarget 위치를 중심으로 폭발
        Vector3 explosionCenter = hitTarget.transform.position;  // ✅ 변경

        List<MonsterBase> monsters = MonsterManager.Instance.GetAliveMonsters();

        float baseDamage = ownerBuilding.stat.attackDamage.Value;
        float aoeDamage = baseDamage * damageRatio;

        foreach (var monster in monsters)
        {
            if (monster == null || !monster.gameObject.activeSelf) continue;

            float distance = Vector3.Distance(explosionCenter, monster.transform.position);
            if (distance <= aoeRadius)
            {
                monster.TakeDamage(aoeDamage);
                LogHelper.Log($"💥 AOE hit: {aoeDamage} damage to {monster.data?.displayName}");
            }
        }

        // ✅ 폭발 이펙트
        EffectManager.Instance.Play("Explosion", explosionCenter);

        TriggerAndConsume(null);
    }
}
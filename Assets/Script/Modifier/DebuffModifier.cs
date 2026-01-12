using System;

/// <summary>
/// 총알 적중 시 타겟 공격력 감소
/// </summary>
public class DebuffModifier : EventModifier
{
    private float debuffRatio;
    private BuildingBase ownerBuilding;

    public DebuffModifier(string id, string name, float ratio) : base(id, name)
    {
        debuffRatio = ratio;
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
        if (hitTarget == null) return;

        // 타겟의 공격력 감소
        float reduction = hitTarget.attackDamage.Value * debuffRatio;
        hitTarget.attackDamage.Value -= reduction;

        LogHelper.Log($"🔻 Debuff applied: -{reduction} attack to {hitTarget.data?.displayName}");

        TriggerAndConsume(null);
    }
}
using System;

/// <summary>
/// 공격 시 총알 추가 발사
/// </summary>
public class DoubleBulletModifier : EventModifier
{
    private BuildingBase ownerBuilding;

    public DoubleBulletModifier(string id, string name) : base(id, name)
    {
    }

    public override void OnAdd()
    {
        base.OnAdd();

        // ✅ 총알 개수 증가
        if (ownerBuilding != null)
        {
            ownerBuilding.stat.bulletCountPerAttack.Value++;
            LogHelper.Log($"✅ BulletCount increased: {ownerBuilding.stat.bulletCountPerAttack.Value}");
        }
    }

    public override void BindEvents(object owner)
    {
        if (owner is BuildingBase building)
        {
            ownerBuilding = building;
        }
    }

    public override void UnbindEvents(object owner)
    {
        // 아무것도 안 함
    }
}
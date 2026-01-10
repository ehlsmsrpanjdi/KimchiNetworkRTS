using System;

// ========== Event Modifier (이벤트 바인딩 효과) ==========
public interface IEventModifier : IModifier
{
    void BindEvents(object owner);
    void UnbindEvents(object owner);
}

public class EventModifier : ModifierBase, IEventModifier
{
    protected Action onAttackAction;
    protected Action onHitAction;
    protected Action onDamagedAction;

    public EventModifier(string id, string name) : base(id, name)
    {
    }

    public virtual void BindEvents(object owner)
    {
        // 하위 클래스에서 구현
    }

    public virtual void UnbindEvents(object owner)
    {
        // 하위 클래스에서 구현
    }

    // 하위 클래스에서 이벤트 액션 설정
    protected void SetOnAttack(Action action) => onAttackAction = action;
    protected void SetOnHit(Action action) => onHitAction = action;
    protected void SetOnDamaged(Action action) => onDamagedAction = action;

    // 이벤트 발동 시 횟수 소모
    protected void TriggerAndConsume(Action action)
    {
        action?.Invoke();
        ConsumeUse();
    }
}
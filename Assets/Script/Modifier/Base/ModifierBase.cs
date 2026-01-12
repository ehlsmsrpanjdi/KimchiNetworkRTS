using UnityEngine;

// ========== 기본 Modifier 인터페이스 ==========
public interface IModifier
{
    void OnAdd();
    void OnRemove();
    bool ShouldRemove();
}

// ========== 기본 Modifier 추상 클래스 ==========
public abstract class ModifierBase : IModifier
{
    public string modifierID;
    public string modifierName;

    // 지속 시간 (0이면 영구)
    protected float duration;
    protected float currentTime;

    // 사용 횟수 (0이면 무제한)
    protected int useCount;
    protected int currentCount;

    public ModifierBase(string id, string name)
    {
        modifierID = id;
        modifierName = name;
        duration = 0f;
        useCount = 0;
    }

    public void SetDuration(float time)
    {
        duration = time;
        currentTime = time;
    }

    public void SetUseCount(int count)
    {
        useCount = count;
        currentCount = count;
    }

    public virtual void OnAdd()
    {
        currentTime = duration;
        currentCount = useCount;
    }

    public virtual void OnRemove()
    {
        // 정리 작업
    }

    public virtual bool ShouldRemove()
    {
        // 시간 체크
        if (duration > 0f)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
                return true;
        }

        // 횟수 체크
        if (useCount > 0 && currentCount <= 0)
            return true;

        return false;
    }

    // 횟수 소모
    protected void ConsumeUse()
    {
        if (useCount > 0)
            currentCount--;
    }
}
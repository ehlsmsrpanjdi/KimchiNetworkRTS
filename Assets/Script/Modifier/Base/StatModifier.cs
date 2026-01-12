// ========== Stat Modifier (스탯 증가/감소) ==========
public interface IStatModifier : IModifier
{
    float ModifyStat(float baseValue);
}

public class StatModifier : ModifierBase, IStatModifier
{
    public enum ModifierType
    {
        Additive,      // 더하기 (+10)
        Multiplicative // 곱하기 (x1.2 = +20%)
    }

    public ModifierType type;
    public float value;

    public StatModifier(string id, string name, ModifierType modType, float val)
        : base(id, name)
    {
        type = modType;
        value = val;
    }

    public float ModifyStat(float baseValue)
    {
        switch (type)
        {
            case ModifierType.Additive:
                return baseValue + value;
            case ModifierType.Multiplicative:
                return baseValue * (1f + value);
            default:
                return baseValue;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class ModifierManager
{
    // Stat Modifiers
    private List<IStatModifier> statModifiers = new List<IStatModifier>();

    // Event Modifiers
    private List<IEventModifier> eventModifiers = new List<IEventModifier>();

    private object owner; // Building or Player

    public ModifierManager(object ownerObject)
    {
        owner = ownerObject;
    }

    // ========== Stat Modifier 관리 ==========
    public void AddStatModifier(IStatModifier modifier)
    {
        modifier.OnAdd();
        statModifiers.Add(modifier);
    }

    public void RemoveStatModifier(IStatModifier modifier)
    {
        modifier.OnRemove();
        statModifiers.Remove(modifier);
    }

    public float GetModifiedStat(float baseStat)
    {
        float result = baseStat;
        foreach (var mod in statModifiers)
        {
            result = mod.ModifyStat(result);
        }
        return result;
    }

    // ========== Event Modifier 관리 ==========
    public void AddEventModifier(IEventModifier modifier)
    {
        modifier.OnAdd();
        modifier.BindEvents(owner);
        eventModifiers.Add(modifier);
    }

    public void RemoveEventModifier(IEventModifier modifier)
    {
        modifier.UnbindEvents(owner);
        modifier.OnRemove();
        eventModifiers.Remove(modifier);
    }

    // ========== Update (시간/횟수 체크) ==========
    public void Update()
    {
        // Stat Modifiers 체크
        for (int i = statModifiers.Count - 1; i >= 0; i--)
        {
            if (statModifiers[i].ShouldRemove())
            {
                RemoveStatModifier(statModifiers[i]);
            }
        }

        // Event Modifiers 체크
        for (int i = eventModifiers.Count - 1; i >= 0; i--)
        {
            if (eventModifiers[i].ShouldRemove())
            {
                RemoveEventModifier(eventModifiers[i]);
            }
        }
    }

    public void Clear()
    {
        // Event Modifier 언바인딩
        for (int i = eventModifiers.Count - 1; i >= 0; i--)
        {
            eventModifiers[i].UnbindEvents(owner);
        }

        statModifiers.Clear();
        eventModifiers.Clear();
    }
}
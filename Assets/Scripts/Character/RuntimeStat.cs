using System.Collections.Generic;
using UnityEngine;

public class RuntimeStat
{
    public float BaseValue { get; private set; }
    public float CurrentValue { get; private set; }

    public List<StatModifier> Modifiers { get; } = new();

    public RuntimeStat(float baseValue)
    {
        this.BaseValue = baseValue;
        CurrentValue = baseValue;
    }

    public void AddModifier(StatModifier modifier)
    {
        Modifiers.Add(modifier);
        CurrentValue += modifier.delta;
    }

    public void Reset()
    {
        CurrentValue = BaseValue;
        Modifiers.Clear();
    }
}

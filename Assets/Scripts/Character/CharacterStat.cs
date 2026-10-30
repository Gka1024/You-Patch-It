using System;
using UnityEngine;

[Serializable]
public class CharacterStat
{
    public CharacterStatType statType;
    public float value;
}

public enum CharacterStatType
{
    Attack,
    Magic,
    Health,
    Defence,
    MagicResist,
    MoveSpeed,
    AttackSpeed,
    AttackRange,
    HealthRegen,

    GainMana,
    MaxMana,
    ManaCost,
    SkillCoefficient
}
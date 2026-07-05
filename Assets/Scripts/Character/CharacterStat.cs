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
    Health,
    Defence,
    MoveSpeed,
    AttackSpeed,
    AttackRange,

    GainManaPerSecond,
    GainManaPerAttack,
    MaxMana,
    ManaCost,
    SkillCoefficient
}
using System.Collections.Generic;
using UnityEngine;

public enum CharacterRole
{
    Warrior,
    Archer,
    Mage,
    Assassin,
    Tank,
    Support
}

[CreateAssetMenu(menuName = "ScriptableObject/Character")]
public class Character : ScriptableObject
{
    public int id;
    public string characterName;

    public CharacterRole role;

    public List<CharacterStat> stats = new();

    // 직업 기본 수정 가능 스탯
    public List<CharacterStatType> defaultEditableStats = new();
}
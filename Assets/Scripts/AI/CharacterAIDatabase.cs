using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/AI Database")]
public class CharacterAIDatabase : ScriptableObject
{
    [Serializable]
    public class RoleAI
    {
        public CharacterRole role;
        public BattleAI battleAI;
    }

    [SerializeField] private List<RoleAI> roleAIs = new();

    public BattleAI GetAI(CharacterRole role)
    {
        RoleAI entry = roleAIs.Find(item => item.role == role);
        return entry != null ? entry.battleAI : null;
    }
}
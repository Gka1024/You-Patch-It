using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Miko")]
public class CharacterSkill_Miko : CharacterSkill
{
    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        foreach (BattleCharacter ally in targets)
        {
            ally.AddModifier(new BattleStatModifier(CharacterStatType.AttackSpeed, BattleStatModifierType.Percent, Mathf.Clamp(coefficient - 1, 0, coefficient), 5));
            ally.AddModifier(new BattleStatModifier(CharacterStatType.MoveSpeed, BattleStatModifierType.Percent, Mathf.Clamp(coefficient - 1, 0, coefficient), 5));
        }
    }
}
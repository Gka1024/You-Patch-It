using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Berserker")]
public class CharacterSkill_Berserker : CharacterSkill
{
    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        self.AddModifier(new BattleStatModifier(CharacterStatType.Attack, BattleStatModifierType.Percent, Mathf.Clamp(coefficient - 1, 0, coefficient), 5));
        self.AddModifier(new BattleStatModifier(CharacterStatType.AttackSpeed, BattleStatModifierType.Percent, Mathf.Clamp(coefficient - 1, 0, coefficient), 5));
        self.AddModifier(new BattleStatModifier(CharacterStatType.MoveSpeed, BattleStatModifierType.Percent, Mathf.Clamp(coefficient - 1, 0, coefficient), 5));
    }
}
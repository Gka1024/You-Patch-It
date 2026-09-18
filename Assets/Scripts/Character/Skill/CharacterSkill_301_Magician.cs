using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Magician")]
public class CharacterSkill_Magician : CharacterSkill
{
    protected override float GetAreaRange(BattleCharacter self, float coefficient)
    {
        return self.GetStat(CharacterStatType.AttackRange) * (coefficient - 1);
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        foreach (BattleCharacter character in targets)
        {
            BattleActionExecutor.DealDamage(self, character, self.GetStat(CharacterStatType.Attack) * coefficient);
        }

    }
}
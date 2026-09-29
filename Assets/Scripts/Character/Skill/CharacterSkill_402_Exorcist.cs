using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Exorcist")]
public class CharacterSkill_Exorcist : CharacterSkill
{
    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random);

        if (targets.Count == 0)
            return;

        BattleCharacter target = targets[0];

        float damage = self.GetStat(CharacterStatType.Attack) * coefficient;
        BattleActionExecutor.DealDamage(self, target, damage);

        target.AddModifier(new BattleStatModifier(CharacterStatType.Defence, BattleStatModifierType.Percent, coefficient * 10f, 5f, self));
    }
}
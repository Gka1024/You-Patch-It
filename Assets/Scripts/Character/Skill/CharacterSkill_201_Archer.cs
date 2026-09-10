using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Archer")]
public class CharacterSkill_Archer : CharacterSkill
{
    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random);

        if (targets.Count == 0)
            return;

        BattleCharacter target = targets[0];

        float damage = self.GetStat(CharacterStatType.Attack) * coefficient;
        BattleActionExecutor.DealDamage(self, target, damage);

        BattleActionExecutor.MoveAway(self, target, self.GetStat(CharacterStatType.MoveSpeed) * coefficient);
    }
}
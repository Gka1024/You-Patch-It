using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/SwordSaint")]
public class CharacterSkill_105_SwordSaint_Skill : CharacterSkill
{
    public override void OnAttackHit(BattleCharacter self)
    {
        self.skillStack++;
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        if (self.skillStack <= 0)
            return;

        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        if (targets.Count == 0)
            return;

        BattleCharacter target = targets[0];

        float damage = self.GetStat(CharacterStatType.Attack) * coefficient * self.skillStack;

        BattleActionExecutor.DealDamage(self, target, damage);

        self.skillStack = 0;
    }
}
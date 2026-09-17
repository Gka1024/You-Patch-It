using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Warrior")]
public class CharacterSkill_Warrior : CharacterSkill
{
    public override void Execute(BattleCharacter self, List<BattleCharacter> enemies, List<BattleCharacter> allies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, enemies, allies, random);

        if (targets.Count == 0)
            return;

        BattleCharacter target = targets[0];

        float damage = self.GetStat(CharacterStatType.Attack) * coefficient;
        BattleActionExecutor.DealDamage(self, target, damage);

        float shield = self.GetStat(CharacterStatType.Health) * (coefficient - 1) * 0.1f;
        BattleActionExecutor.AddShield(self, shield, 5);

    }
}
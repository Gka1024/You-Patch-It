using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Bard")]
public class CharacterSkill_Bard : CharacterSkill
{
    private const float BuffDuration = 3f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random);

        foreach (BattleCharacter target in targets)
        {
            target.AddModifier(new BattleStatModifier(CharacterStatType.Defence, BattleStatModifierType.Flat, coefficient * 5, coefficient * BuffDuration));
            BattleActionExecutor.AddShield(target, self.GetStat(CharacterStatType.Health) * coefficient, coefficient * BuffDuration);

            /* 힐하는 부분인데 스킬이 너무 사기라서 제외했습니다
           // float maxHealth = target.runtimeCharacter.GetStat(CharacterStatType.Health);
           // target.currentHealth = Math.Min(target.currentHealth + self.GetStat(CharacterStatType.Health) * coefficient * 0.01f, maxHealth);
            */
        }

    }
}
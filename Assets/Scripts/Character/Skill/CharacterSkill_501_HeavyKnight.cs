using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/HeavyKnight")]
public class CharacterSkill_HeavyKnight : CharacterSkill
{
    [SerializeField] private float tauntDuration = 3f;

    protected override float GetAreaRange(BattleCharacter self, float coefficient)
    {
        return self.GetStat(CharacterStatType.AttackRange) * coefficient;
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        for (int i = 0; i < targets.Count; i++)
        {
            BattleActionExecutor.AddTaunt(targets[i], self, tauntDuration);
        }

        BattleActionExecutor.AddShield(self, self.GetStat(CharacterStatType.Health) * (coefficient - 1));
    }
}
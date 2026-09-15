using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Summoner")]
public class CharacterSkill_Summoner : CharacterSkill
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float damageMultiplier = 0.5f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random);

        if (targets.Count == 0)
        {
            return;
        }

        float damage = self.GetStat(CharacterStatType.Attack) * coefficient * damageMultiplier;

        BattleActionExecutor.AddDamageOverTime(self, targets[0], damage, duration, tickInterval);
    }
}
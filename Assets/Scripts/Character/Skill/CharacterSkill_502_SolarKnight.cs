using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/SolarKnight")]
public class CharacterSkill_SolarKnight : CharacterSkill
{
    [SerializeField] private float shieldRange = 5f;
    [SerializeField] private float shieldCoefficient = 1f;
    [SerializeField] private float shieldDuration = 5f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        float shieldAmount = self.GetStat(CharacterStatType.Health) * coefficient;

        foreach (BattleCharacter ally in targets)
        {
            BattleActionExecutor.AddShield(ally, shieldAmount, shieldDuration);
        }
    }
}
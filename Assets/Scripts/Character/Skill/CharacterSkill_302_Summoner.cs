using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Summoner")]
public class CharacterSkill_Summoner : CharacterSkill
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float damageMultiplier = 0.5f;

    private float CalculateDamage(float attack, float coefficient)
    {
        return attack * coefficient * damageMultiplier;
    }

    private int CalculateDamageCount()
    {
        return Mathf.FloorToInt(duration / tickInterval);
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random);

        if (targets.Count == 0)
        {
            return;
        }

        float damage = CalculateDamage(self.GetStat(CharacterStatType.Magic), coefficient);

        BattleActionExecutor.AddDamageOverTime(self, targets[0], damage, duration, tickInterval, true);
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attack = character.GetStat(CharacterStatType.Magic);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        float damage = CalculateDamage(attack, coefficient);
        int damageCount = CalculateDamageCount();
        float totalDamage = damage * damageCount;

        skillEffectInfos.Add(new SkillEffectInfo("1회 피해량", damage));
        skillEffectInfos.Add(new SkillEffectInfo("지속 시간", duration, "초"));
        skillEffectInfos.Add(new SkillEffectInfo("피해 간격", tickInterval, "초"));
        skillEffectInfos.Add(new SkillEffectInfo("최대 총 피해량", totalDamage));
    }
}
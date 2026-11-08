using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Magician")]
public class CharacterSkill_Magician : CharacterSkill
{
    private float CalculateDamage(float attack, float coefficient)
    {
        return attack * coefficient;
    }

    private float CalculateAreaRange(float attackRange, float coefficient)
    {
        return attackRange * (coefficient - 1f);
    }

    protected override float GetAreaRange(BattleCharacter self, float coefficient)
    {
        return CalculateAreaRange(self.GetStat(CharacterStatType.AttackRange), coefficient);
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        foreach (BattleCharacter character in targets)
        {
            float damage = CalculateDamage(self.GetStat(CharacterStatType.Magic), coefficient);
            BattleActionExecutor.DealDamage(self, character, damage, true);
        }
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attack = character.GetStat(CharacterStatType.Magic);
        float attackRange = character.GetStat(CharacterStatType.AttackRange);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        float damage = CalculateDamage(attack, coefficient);
        float areaRange = CalculateAreaRange(attackRange, coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("피해량", damage));
        skillEffectInfos.Add(new SkillEffectInfo("범위 증가량", areaRange));
    }
}
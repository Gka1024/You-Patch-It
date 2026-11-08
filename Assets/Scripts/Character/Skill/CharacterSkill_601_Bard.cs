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

        float health = self.GetStat(CharacterStatType.Health);
        float magic = self.GetStat(CharacterStatType.Magic);

        float defenceIncrease = CalculateDefenceIncrease(magic, coefficient);
        float shieldAmount = CalculateShield(health, magic, coefficient);

        foreach (BattleCharacter target in targets)
        {
            target.AddModifier(new BattleStatModifier(CharacterStatType.Defence, BattleStatModifierType.Flat, defenceIncrease, coefficient * BuffDuration));
            BattleActionExecutor.AddShield(target, shieldAmount, coefficient * BuffDuration);

            /* 힐하는 부분인데 스킬이 너무 사기라서 제외했습니다
            // float maxHealth = target.runtimeCharacter.GetStat(CharacterStatType.Health);
            // target.currentHealth = Math.Min(target.currentHealth + self.GetStat(CharacterStatType.Health) * coefficient * 0.01f, maxHealth);
            */
        }
    }

    private float CalculateDefenceIncrease(float magic, float coefficient)
    {
        return (1 + magic * 0.01f) * coefficient * 3f;
    }

    private float CalculateShield(float health, float magic, float coefficient)
    {
        return health * (1 + magic * 0.01f) * coefficient;
    }

    private float CalculateDuration(float coefficient)
    {
        return coefficient * BuffDuration;
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float health = character.GetStat(CharacterStatType.Health);
        float magic = character.GetStat(CharacterStatType.Magic);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        skillEffectInfos.Add(new SkillEffectInfo("방어력 증가", CalculateDefenceIncrease(magic, coefficient)));
        skillEffectInfos.Add(new SkillEffectInfo("보호막 흡수량", CalculateShield(health, magic, coefficient)));
        skillEffectInfos.Add(new SkillEffectInfo("효과 지속 시간", CalculateDuration(coefficient), "초"));
    }
}
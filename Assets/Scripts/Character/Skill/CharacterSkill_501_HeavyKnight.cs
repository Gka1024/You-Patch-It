using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/HeavyKnight")]
public class CharacterSkill_HeavyKnight : CharacterSkill
{
    [SerializeField] private float tauntDuration = 3f;

    private float CalculateAreaRange(float attackRange, float coefficient)
    {
        return attackRange * coefficient;
    }

    private float CalculateShield(float health, float coefficient)
    {
        return Mathf.Max(0f, health * (coefficient - 1f) * 0.3f);
    }

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

        float shieldAmount = self.GetStat(CharacterStatType.Health) * (coefficient - 1) * 0.3f;

        BattleActionExecutor.AddShield(self, shieldAmount, tauntDuration);
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attackRange = character.GetStat(CharacterStatType.AttackRange);
        float health = character.GetStat(CharacterStatType.Health);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        skillEffectInfos.Add(new SkillEffectInfo("도발 범위", CalculateAreaRange(attackRange, coefficient)));
        skillEffectInfos.Add(new SkillEffectInfo("보호막 흡수량", CalculateShield(health, coefficient)));
        skillEffectInfos.Add(new SkillEffectInfo("도발 및 보호막 지속 시간", tauntDuration, "초"));
    }
}
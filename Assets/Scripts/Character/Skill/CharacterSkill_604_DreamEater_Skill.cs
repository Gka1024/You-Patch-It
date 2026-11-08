using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/DreamEater")]
public class CharacterSkill_604_DreamEater_Skill : CharacterSkill
{
    [Header("Debuff Settings")]
    [SerializeField] private float duration = 5f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        float reduction = CalculateReduction(coefficient);

        foreach (BattleCharacter enemy in enemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            enemy.AddModifier(new BattleStatModifier(CharacterStatType.Attack, BattleStatModifierType.Percent, -reduction, duration));
            enemy.AddModifier(new BattleStatModifier(CharacterStatType.AttackSpeed, BattleStatModifierType.Percent, -reduction, duration));
        }
    }

    private float CalculateReduction(float coefficient)
    {
        return (1f + coefficient) * 0.1f;
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        float reduction = CalculateReduction(coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("공격력 감소", reduction * 100f, "%"));
        skillEffectInfos.Add(new SkillEffectInfo("공격 속도 감소", reduction * 100f, "%"));
        skillEffectInfos.Add(new SkillEffectInfo("효과 지속 시간", duration, "초"));
    }
}

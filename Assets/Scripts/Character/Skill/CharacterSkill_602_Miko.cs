using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Miko")]
public class CharacterSkill_Miko : CharacterSkill
{
    private const float BuffDuration = 5f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        foreach (BattleCharacter ally in targets)
        {
            ally.AddModifier(new BattleStatModifier(CharacterStatType.AttackSpeed, BattleStatModifierType.Percent, CalculateBuff(coefficient), BuffDuration));
            ally.AddModifier(new BattleStatModifier(CharacterStatType.MoveSpeed, BattleStatModifierType.Percent, CalculateBuff(coefficient), BuffDuration));
        }
    }

    private float CalculateBuff(float coefficient)
    {
        return Mathf.Clamp(coefficient - 1f, 0f, coefficient);
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);
        float buff = CalculateBuff(coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("공격 속도 증가", buff * 100f, "%"));
        skillEffectInfos.Add(new SkillEffectInfo("이동 속도 증가", buff * 100f, "%"));
        skillEffectInfos.Add(new SkillEffectInfo("효과 지속 시간", BuffDuration, "초"));
    }
}
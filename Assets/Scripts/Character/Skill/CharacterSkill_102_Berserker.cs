using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Berserker")]
public class CharacterSkill_Berserker : CharacterSkill
{
    private const float buffDuration = 5f;

    private float CalculateBuff(float coefficient)
    {
        return Mathf.Clamp(coefficient - 1f, 0f, coefficient);
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        float buff = CalculateBuff(coefficient);

        self.AddModifier(new BattleStatModifier(CharacterStatType.Attack, BattleStatModifierType.Percent, buff, buffDuration));
        self.AddModifier(new BattleStatModifier(CharacterStatType.AttackSpeed, BattleStatModifierType.Percent, buff, buffDuration));
        self.AddModifier(new BattleStatModifier(CharacterStatType.MoveSpeed, BattleStatModifierType.Percent, buff, buffDuration));
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);
        float buff = CalculateBuff(coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("공격력 증가", buff * 100f, "%"));
        skillEffectInfos.Add(new SkillEffectInfo("공격 속도 증가", buff * 100f, "%"));
        skillEffectInfos.Add(new SkillEffectInfo("이동 속도 증가", buff * 100f, "%"));
        skillEffectInfos.Add(new SkillEffectInfo("버프 지속 시간", buffDuration, "초"));
    }
}
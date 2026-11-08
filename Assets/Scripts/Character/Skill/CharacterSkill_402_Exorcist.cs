using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Exorcist")]
public class CharacterSkill_Exorcist : CharacterSkill
{
    private const float DefenceReductionDuration = 5f;

    private float CalculateDamage(float attack, float magic, float coefficient)
    {
        return (1 + 0.1f * magic) * attack * coefficient;
    }

    private float CalculateDefenceReduction(float coefficient)
    {
        return Mathf.Clamp((coefficient - 1f) / 2f, 0, 0.5f);
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random);

        if (targets.Count == 0)
            return;

        BattleCharacter target = targets[0];

        float damage = CalculateDamage(self.GetStat(CharacterStatType.Attack), self.GetStat(CharacterStatType.Magic), coefficient);
        BattleActionExecutor.DealDamage(self, target, damage);

        float defenceReduction = CalculateDefenceReduction(coefficient);

        target.AddModifier(new BattleStatModifier(CharacterStatType.Defence, BattleStatModifierType.Percent, -defenceReduction, DefenceReductionDuration, self));
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attack = character.GetStat(CharacterStatType.Attack);
        float magic = character.GetStat(CharacterStatType.Magic);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        float damage = CalculateDamage(attack, magic, coefficient);
        float defenceReduction = CalculateDefenceReduction(coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("피해량", damage));
        skillEffectInfos.Add(new SkillEffectInfo("방어력 감소", defenceReduction * 100f, "%"));
        skillEffectInfos.Add(new SkillEffectInfo("방어력 감소 지속 시간", DefenceReductionDuration, "초"));
    }
}
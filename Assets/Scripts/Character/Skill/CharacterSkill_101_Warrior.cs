using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Warrior")]
public class CharacterSkill_Warrior : CharacterSkill
{
    private float CalculateDamage(float attack, float coefficient)
    {
        return attack * (coefficient + 0.5f);
    }

    private float CalculateShield(float health, float coefficient)
    {
        return health * (coefficient - 1f) * 0.1f;
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> enemies, List<BattleCharacter> allies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random);

        if (targets.Count == 0)
            return;

        BattleCharacter target = targets[0];

        float damage = CalculateDamage(self.GetStat(CharacterStatType.Attack), coefficient);
        BattleActionExecutor.DealDamage(self, target, damage);

        float shield = CalculateShield(self.GetStat(CharacterStatType.Health), coefficient);
        BattleActionExecutor.AddShield(self, shield, 5f);
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attack = character.GetStat(CharacterStatType.Attack);
        float health = character.GetStat(CharacterStatType.Health);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        skillEffectInfos.Add(new SkillEffectInfo("피해량", CalculateDamage(attack, coefficient)));
        skillEffectInfos.Add(new SkillEffectInfo("보호막", CalculateShield(health, coefficient)));
        skillEffectInfos.Add(new SkillEffectInfo("보호막 지속 시간", 5f, "초"));
    }
}
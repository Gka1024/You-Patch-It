using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/SolarKnight")]
public class CharacterSkill_SolarKnight : CharacterSkill
{
    [SerializeField] private float shieldDuration = 5f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        float shieldAmount = CalculateShield(self.GetStat(CharacterStatType.Health), self.GetStat(CharacterStatType.Magic), coefficient);

        foreach (BattleCharacter ally in targets)
        {
            BattleActionExecutor.AddShield(ally, shieldAmount, shieldDuration);
        }
    }

    private float CalculateShield(float health, float magic, float coefficient)
    {
        return health * coefficient * (1 + magic * 0.01f);
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float health = character.GetStat(CharacterStatType.Health);
        float magic = character.GetStat(CharacterStatType.Magic);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        skillEffectInfos.Add(new SkillEffectInfo("보호막 흡수량", CalculateShield(health, magic, coefficient)));
        skillEffectInfos.Add(new SkillEffectInfo("보호막 지속 시간", shieldDuration, "초"));
    }
}
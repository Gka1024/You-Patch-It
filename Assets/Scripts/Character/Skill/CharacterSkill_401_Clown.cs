using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Assasin")]
public class CharacterSkill_Clown : CharacterSkill
{
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float stunDuration = 1f;

    private float CalculateMoveDistance(float moveSpeed, float coefficient)
    {
        return moveSpeed * coefficient;
    }

    private float CalculateDamage(float attack, float coefficient)
    {
        return attack * coefficient * damageMultiplier;
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random);

        if (targets.Count == 0)
            return;

        BattleCharacter target = targets[0];

        float moveDistance = CalculateMoveDistance(self.GetStat(CharacterStatType.MoveSpeed), coefficient);
        BattleActionExecutor.MoveTowards(self, target, moveDistance);

        float damage = CalculateDamage(self.GetStat(CharacterStatType.Attack), coefficient);
        BattleActionExecutor.DealDamage(self, target, damage);

        BattleActionExecutor.AddStun(target, stunDuration);
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attack = character.GetStat(CharacterStatType.Attack);
        float moveSpeed = character.GetStat(CharacterStatType.MoveSpeed);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        float moveDistance = CalculateMoveDistance(moveSpeed, coefficient);
        float damage = CalculateDamage(attack, coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("이동 거리", moveDistance));
        skillEffectInfos.Add(new SkillEffectInfo("피해량", damage));
        skillEffectInfos.Add(new SkillEffectInfo("기절 지속 시간", stunDuration, "초"));
    }
}
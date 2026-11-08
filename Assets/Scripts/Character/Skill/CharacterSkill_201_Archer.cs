using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Archer")]
public class CharacterSkill_Archer : CharacterSkill
{
    private float CalculateDamage(float attack, float coefficient)
    {
        return attack * coefficient;
    }

    private float CalculateMoveDistance(float moveSpeed, float coefficient)
    {
        return moveSpeed * coefficient;
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random);

        if (targets.Count == 0)
            return;

        BattleCharacter target = targets[0];

        float damage = CalculateDamage(self.GetStat(CharacterStatType.Attack), coefficient);
        BattleActionExecutor.DealDamage(self, target, damage);

        if (Mathf.Abs(target.position - self.position) > self.aiState.PreferredDistance)
        {
            float moveDistance = CalculateMoveDistance(self.GetStat(CharacterStatType.MoveSpeed), coefficient);
            BattleActionExecutor.MoveAway(self, target, moveDistance);
        }
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attack = character.GetStat(CharacterStatType.Attack);
        float moveSpeed = character.GetStat(CharacterStatType.MoveSpeed);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        float damage = CalculateDamage(attack, coefficient);
        float moveDistance = CalculateMoveDistance(moveSpeed, coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("피해량", damage));
        skillEffectInfos.Add(new SkillEffectInfo("후퇴 거리", moveDistance));
    }
}
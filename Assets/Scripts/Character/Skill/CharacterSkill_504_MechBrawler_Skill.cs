using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/MechBrawler")]
public class CharacterSkill_504_MechBrawler_Skill : CharacterSkill
{
    [Header("Pull Settings")]
    [SerializeField] private float pullDistance = 0.5f;

    [Header("Stun Settings")]
    [SerializeField] private float baseMaxRange = 3f;
    [SerializeField] private float baseStunDuration = 1f;
    [SerializeField] private float stunCoefficient = 0.5f;

    private float CalculateMaxRange(float attackRange, float coefficient)
    {
        return baseMaxRange + attackRange * coefficient;
    }

    private float CalculateStunDuration(float coefficient)
    {
        return baseStunDuration + stunCoefficient * coefficient;
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        float maxRange = CalculateMaxRange(
            self.GetStat(CharacterStatType.AttackRange),
            coefficient
        );

        float stunDuration = CalculateStunDuration(coefficient);

        BattleCharacter target = GetPullTarget(self, enemies, maxRange);

        if (target == null)
            return;

        float direction = Mathf.Sign(self.position - target.position);

        // 메카닉 앞의 지정된 거리까지 적을 끌어온다.
        target.position = self.position - direction * pullDistance;

        BattleActionExecutor.AddStun(target, stunDuration);
    }

    private BattleCharacter GetPullTarget(BattleCharacter self, List<BattleCharacter> enemies, float maxRange)
    {
        BattleCharacter closestTarget = null;
        float closestDistance = maxRange;

        foreach (BattleCharacter enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            float distance = Mathf.Abs(enemy.position - self.position);

            if (distance > closestDistance)
                continue;

            closestDistance = distance;
            closestTarget = enemy;
        }

        return closestTarget;
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);
        float attackRange = character.GetStat(CharacterStatType.AttackRange);

        float maxRange = CalculateMaxRange(attackRange, coefficient);
        float stunDuration = CalculateStunDuration(coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("최대 탐색 거리", maxRange));
        skillEffectInfos.Add(new SkillEffectInfo("끌어당긴 후 거리", pullDistance));
        skillEffectInfos.Add(new SkillEffectInfo("기절 지속 시간", stunDuration, "초"));
    }
}
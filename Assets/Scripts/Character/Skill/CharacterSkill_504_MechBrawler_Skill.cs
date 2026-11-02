using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/MechBrawler")]
public class CharacterSkill_504_MechBrawler_Skill : CharacterSkill
{
    [Header("Pull Settings")]
    [SerializeField] private float maxRange = 5f;
    [SerializeField] private float pullDistance = 0.5f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        BattleCharacter target = GetPullTarget(self, enemies);

        if (target == null)
            return;

        float direction = Mathf.Sign(self.position - target.position);

        // 메카닉 앞의 지정된 거리까지 적을 끌어온다.
        target.position = self.position - direction * pullDistance;
    }

    private BattleCharacter GetPullTarget(BattleCharacter self, List<BattleCharacter> enemies)
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
}
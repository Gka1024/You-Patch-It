using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/DarkKnight")]
public class CharacterSkill_505_DarkKnight_Skill : CharacterSkill
{
    [Header("Skill Settings")]
    [SerializeField] private float healthRatio = 0.2f;
    [SerializeField] private float attackRange = 2f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        float maxHealth = self.GetStat(CharacterStatType.Health);
        float healthCost = maxHealth * healthRatio;

        // 현재 체력이 소모량보다 적으면 체력을 회복한다.
        if (self.currentHealth < healthCost)
        {
            self.currentHealth = Mathf.Min(maxHealth, self.currentHealth + healthCost);
            return;
        }

        // 최대 체력의 20%를 소모한다.
        self.currentHealth -= healthCost;

        // 자신의 체력 소모량과 동일한 피해를 주변 적 전체에게 준다.
        foreach (BattleCharacter enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            float distance = Mathf.Abs(enemy.position - self.position);

            if (distance > attackRange)
                continue;

            BattleActionExecutor.DealDamage(self, enemy, healthCost);
        }
    }
}

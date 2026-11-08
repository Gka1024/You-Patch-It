using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/DarkKnight")]
public class CharacterSkill_505_DarkKnight_Skill : CharacterSkill
{
    [Header("Skill Settings")]
    [SerializeField] private float healthCostRatio = 0.2f;
    [SerializeField] private float healRatio = 0.1f;
    [SerializeField] private float healThreshold = 0.3f;
    [SerializeField] private float attackRange = 2f;

    private float CalculateHealthCost(float maxHealth)
    {
        return maxHealth * healthCostRatio;
    }

    private float CalculateHealing(float maxHealth, float coefficient)
    {
        return maxHealth * healRatio * coefficient;
    }

    private float CalculateDamage(float maxHealth, float coefficient)
    {
        float healthCost = CalculateHealthCost(maxHealth);
        return healthCost * Mathf.Max(0f, coefficient - 1f);
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        float maxHealth = self.GetStat(CharacterStatType.Health);

        // 현재 체력이 최대 체력의 30% 미만이면 체력을 회복한다.
        if (self.currentHealth < maxHealth * healThreshold)
        {
            float healing = CalculateHealing(maxHealth, coefficient);

            self.currentHealth = Mathf.Min(maxHealth, self.currentHealth + healing);
            return;
        }

        // 최대 체력의 20%를 소모한다.
        float healthCost = CalculateHealthCost(maxHealth);
        self.currentHealth -= healthCost;

        // 소모한 체력에 비례하여 주변 적에게 피해를 준다.
        float damage = CalculateDamage(maxHealth, coefficient);

        foreach (BattleCharacter enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            float distance = Mathf.Abs(enemy.position - self.position);

            if (distance > attackRange)
                continue;

            BattleActionExecutor.DealDamage(self, enemy, damage);
        }
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float maxHealth = character.GetStat(CharacterStatType.Health);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        float healthCost = CalculateHealthCost(maxHealth);
        float healing = CalculateHealing(maxHealth, coefficient);
        float damage = CalculateDamage(maxHealth, coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("[체력 30% 미만] 회복량", healing));
        skillEffectInfos.Add(new SkillEffectInfo("[체력 30% 이상] 체력 소모량", healthCost));
        skillEffectInfos.Add(new SkillEffectInfo("[체력 30% 이상] 주변 적 피해량", damage));
        skillEffectInfos.Add(new SkillEffectInfo("공격 범위", attackRange));
    }
}
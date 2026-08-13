using System;
using UnityEngine;

public static class BattleActionExecutor
{
    private const float TICK = 0.05f;

    public static void ExecuteAction(BattleCharacter self, BattleCharacter enemy, BattleAction action, float tick, System.Random random)
    {
        action = ApplyDecisionAccuracy(self, action, random);

        switch (action)
        {
            case BattleAction.MoveTowards:
                MoveTowards(self, enemy, tick);
                break;

            case BattleAction.MoveAway:
                MoveAway(self, enemy, tick);
                break;

            case BattleAction.Attack:
                Attack(self, enemy);
                break;

            case BattleAction.UseSkill:
                UseSkill(self, enemy);
                break;
        }
    }

    private static void MoveTowards(BattleCharacter self, BattleCharacter enemy, float tick)
    {
        float direction = Mathf.Sign(enemy.position - self.position);
        Move(self, direction, tick);
    }

    private static void MoveAway(BattleCharacter self, BattleCharacter enemy, float tick)
    {
        float direction = -Mathf.Sign(enemy.position - self.position);
        Move(self, direction, tick);
    }

    private static void Move(BattleCharacter self, float direction, float tick)
    {
        float moveDistance = self.GetStat(CharacterStatType.MoveSpeed) * tick;

        self.position += direction * moveDistance;

        self.statistics.moveDistance += moveDistance;
    }

    private static void Attack(BattleCharacter self, BattleCharacter enemy)
    {
        float damage = self.GetStat(CharacterStatType.Attack) * GetDamageMultiplier(self);
        damage *= 100f / (100f + enemy.GetStat(CharacterStatType.Defence));

        self.currentMana += self.runtimeCharacter.GetStat(CharacterStatType.GainMana) / 20f;

        enemy.currentHealth -= damage;

        self.attackCooldown = 1f / self.GetStat(CharacterStatType.AttackSpeed);

        self.actionLockTime = 0.4f / self.GetStat(CharacterStatType.AttackSpeed);

        self.statistics.damageDealt += damage;
        self.statistics.attackCount++;
        enemy.statistics.damageTaken += damage;
    }

    private static void UseSkill(BattleCharacter self, BattleCharacter enemy)
    {
        self.currentMana = 0;

        if (self.skill != null)
        {
            self.skill.Execute(self, enemy, self.runtimeCharacter.GetStat(CharacterStatType.SkillCoefficient));
        }
        else
        {
            Debug.Log($"{self.runtimeCharacter.OriginCharacter.characterName} 가 스킬이 없습니다!");
        }

    }

    private static float GetDamageMultiplier(BattleCharacter self)
    {
        return Mathf.Lerp(0.8f, 1.2f, self.player.ExecutionSkill / 100f);
    }

    private static BattleAction ApplyDecisionAccuracy(BattleCharacter self, BattleAction action, System.Random random)
    {
        float failChance = Mathf.Lerp(0.3f, 0f, self.player.DecisionAccuracy / 100f);

        if (random.NextDouble() > failChance)
        {
            return action;
        }

        BattleAction[] actions = (BattleAction[])Enum.GetValues(typeof(BattleAction));

        return actions[random.Next(actions.Length)];
    }

}

public enum BattleAction
{
    None,

    MoveTowards,
    MoveAway,

    Attack,
    UseSkill
}
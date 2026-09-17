using System;
using System.Collections.Generic;
using UnityEngine;

public static class BattleActionExecutor
{
    private const float TICK = 0.05f;

    public static void ExecuteAction(BattleCharacter self, BattleCharacter enemy, List<BattleCharacter> allies, List<BattleCharacter> enemies, BattleAction action, float tick, System.Random random)
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
                UseSkill(self, allies, enemies, random);
                break;
        }
    }

    public static void MoveTowards(BattleCharacter self, BattleCharacter enemy, float tick)
    {
        float direction = Mathf.Sign(enemy.position - self.position);
        Move(self, direction, tick);

        float directionAfter = Mathf.Sign(enemy.position - self.position);

        if (directionAfter != direction)
        {
            self.position = enemy.position - 0.01f;
        }
    }

    public static void MoveToTarget(BattleCharacter self, BattleCharacter target, float distance)
    {
        float direction = Mathf.Sign(target.position - self.position);

        self.position += direction * distance;

        self.statistics.moveDistance += distance;

        if (Mathf.Sign(target.position - self.position) != direction)
        {
            self.position = target.position - direction * 0.01f;
        }
    }

    public static void MoveAway(BattleCharacter self, BattleCharacter enemy, float tick)
    {
        float direction = -Mathf.Sign(enemy.position - self.position);
        Move(self, direction, tick);
    }

    public static void Move(BattleCharacter self, float direction, float tick)
    {
        float moveDistance = self.GetStat(CharacterStatType.MoveSpeed) * tick;

        self.position += direction * moveDistance;

        self.statistics.moveDistance += moveDistance;
    }

    public static void Attack(BattleCharacter self, BattleCharacter enemy)
    {
        float damage = self.GetStat(CharacterStatType.Attack) * GetDamageMultiplier(self);

        self.currentMana += self.runtimeCharacter.GetStat(CharacterStatType.GainMana) / 40f;

        DealDamage(self, enemy, damage);

        self.attackCooldown = 1f / self.GetStat(CharacterStatType.AttackSpeed);
        self.actionLockTime = 0.4f / self.GetStat(CharacterStatType.AttackSpeed);

        self.statistics.attackCount++;
    }

    public static void UseSkill(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, System.Random random)
    {
        self.currentMana = 0f;

        if (self.skill == null)
        {
            Debug.Log($"{self.runtimeCharacter.OriginCharacter.characterName} 가 스킬이 없습니다!");
            return;
        }

        self.skill.Execute(self, allies, enemies, self.runtimeCharacter.GetStat(CharacterStatType.SkillCoefficient), random);
        self.statistics.skillCount++;
    }

    public static void DealDamage(BattleCharacter attacker, BattleCharacter target, float damage)
    {
        damage *= 100f / (100f + target.GetStat(CharacterStatType.Defence));

        float remainingDamage = damage;

        if (target.currentShield > 0f)
        {
            float absorbedDamage = Mathf.Min(target.currentShield, remainingDamage);

            target.currentShield -= absorbedDamage;
            remainingDamage -= absorbedDamage;
        }

        if (remainingDamage > 0f)
        {
            target.currentHealth -= remainingDamage;
        }

        attacker.statistics.damageDealt += damage;
        target.statistics.damageTaken += damage;
    }

    public static void AddShield(BattleCharacter target, float amount, float time)
    {
        target.AddShield(amount, time);
    }

    public static void AddStun(BattleCharacter target, float duration)
    {
        target.actionLockTime = Mathf.Max(target.actionLockTime, duration);
    }

    public static void AddTaunt(BattleCharacter target, BattleCharacter taunter, float duration)
    {
        target.currentTarget = taunter;
        target.targetUpdateTimer = Mathf.Max(target.targetUpdateTimer, duration);
    }

    public static void AddDamageOverTime(BattleCharacter source, BattleCharacter target, float damage, float duration, float tickInterval)
    {
        target.AddDamageOverTime(new BattleDamageOverTime(source, target, damage, duration, tickInterval, DealDamage));
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
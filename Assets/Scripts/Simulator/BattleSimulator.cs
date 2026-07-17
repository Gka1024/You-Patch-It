using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class BattleSimulator : MonoBehaviour
{
    public static BattleSimulator Instance;
    public PlayerProfile sampleProfile;

    public StatisticsManager statisticsManager;

    private const float tick = 0.05f;
    private const float BATTLE_TIME_LIMIT = 300f;

    private void Awake()
    {
        Instance = this;
    }

    public List<BattleResult> StartSimulation(List<MatchData> matches, System.Random random)
    {
        List<BattleResult> results = new();

        foreach (MatchData match in matches)
        {
            results.Add(StartSimulation(match, SeasonManager.Instance.SeasonSeed));
        }

        return results;
    }

    public BattleResult StartSimulation(MatchData data, int simulationSeed)
    {
        System.Random simulationRandom = new System.Random(simulationSeed);

        int battleSeed = simulationRandom.Next();

        return Simulate(data.redCharacter, data.redPlayer, data.blueCharacter, data.bluePlayer, battleSeed);
    }

    public BattleResult Simulate(RuntimeCharacter redCharacter, RuntimePlayer redPlayer, RuntimeCharacter blueCharacter, RuntimePlayer bluePlayer, int battleSeed)
    {
        System.Random battleRandom = new System.Random(battleSeed);

        BattleStatistics statistics = new BattleStatistics();

        BattleAIState redAI = new BattleAIState(redCharacter.OriginCharacter.battleAI, sampleProfile, battleRandom);

        BattleCharacter red = new BattleCharacter(redCharacter, redPlayer, redAI, 0f);

        BattleAIState blueAI = new BattleAIState(blueCharacter.OriginCharacter.battleAI, sampleProfile, battleRandom);

        BattleCharacter blue = new BattleCharacter(blueCharacter, bluePlayer, blueAI, 10f);

        // BattleCharacter가 생성한 통계를 등록
        statistics.RegisterRed(red.statistics);
        statistics.RegisterBlue(blue.statistics);

        float battleTime = 0f;

        while (!red.IsDead && !blue.IsDead)
        {
            // Tick 감소
            TickCharacter(red);
            TickCharacter(blue);

            // 행동 결정
            BattleAction redAction = BattleAction.None;
            BattleAction blueAction = BattleAction.None;

            if (red.CanAct && red.CanThink)
            {
                red.reactionTimer = GetReactionTime(red);
                redAction = red.aiState.DecideAction(red, blue);
            }

            if (blue.CanAct && blue.CanThink)
            {
                blue.reactionTimer = GetReactionTime(blue);
                blueAction = blue.aiState.DecideAction(blue, red);
            }

            // 행동 실행
            ExecuteAction(red, blue, redAction, tick);
            ExecuteAction(blue, red, blueAction, tick);

            battleTime += tick;

            if (battleTime > BATTLE_TIME_LIMIT)
            {
                Debug.LogWarning($"Battle Timeout (Seed : {battleSeed})");
                break;
            }
        }

        statistics.battleDuration = battleTime;

        red.statistics.survivalTime = battleTime;
        blue.statistics.survivalTime = battleTime;

        RuntimeCharacter winner = null;
        RuntimeCharacter loser = null;

        if (red.IsDead && blue.IsDead)
        {
            // 동시에 죽은 경우
        }
        else if (red.IsDead)
        {
            winner = blue.runtimeCharacter;
            loser = red.runtimeCharacter;
        }
        else
        {
            winner = red.runtimeCharacter;
            loser = blue.runtimeCharacter;
        }

        return new BattleResult(winner, loser, statistics);
    }

    private void TickCharacter(BattleCharacter character)
    {
        character.attackCooldown = Mathf.Max(0, character.attackCooldown - tick);
        character.actionLockTime = Mathf.Max(0, character.actionLockTime - tick);
        character.reactionTimer = Mathf.Max(0, character.reactionTimer - tick);
    }

    private float GetReactionTime(BattleCharacter character)
    {
        float multiflier = Mathf.Lerp(1.4f, 0.6f, character.player.reactionTime / 100f);

        return character.aiState.reactionTime * multiflier;
    }

    private void ExecuteAction(BattleCharacter self, BattleCharacter enemy, BattleAction action, float tick)
    {
        action = ApplyDecisionAccuracy(self, action);

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
        }
    }

    private void MoveTowards(BattleCharacter self, BattleCharacter enemy, float tick)
    {
        float direction = Mathf.Sign(enemy.position - self.position);
        Move(self, direction, tick);
    }

    private void MoveAway(BattleCharacter self, BattleCharacter enemy, float tick)
    {
        float direction = -Mathf.Sign(enemy.position - self.position);
        Move(self, direction, tick);
    }

    private void Move(BattleCharacter self, float direction, float tick)
    {
        float moveDistance = self.moveSpeed * tick;

        self.position += direction * moveDistance;

        self.statistics.moveDistance += moveDistance;
    }

    private void Attack(BattleCharacter self, BattleCharacter enemy)
    {
        float damage = self.attack * GetDamageMultiplier(self);
        damage *= 100f / (100f + enemy.defence);

        enemy.currentHealth -= damage;

        self.attackCooldown = 1f / self.attackSpeed;

        self.actionLockTime = 0.4f / self.attackSpeed;

        self.statistics.damageDealt += damage;
        self.statistics.attackCount++;
        enemy.statistics.damageTaken += damage;
    }

    private float GetDamageMultiplier(BattleCharacter self)
    {
        return Mathf.Lerp(0.8f, 1.2f, self.player.executionSkill / 100f);
    }

    private float GetConsistencyModifier(BattleCharacter self)
    {
        float variance = Mathf.Lerp(0.35f, 0.05f, self.player.consistency / 100f);

        return UnityEngine.Random.Range(1f - variance, 1f + variance);
    }

    private BattleAction ApplyDecisionAccuracy(BattleCharacter self, BattleAction action)
    {
        float failChance = Mathf.Lerp(0.3f, 0f, self.player.decisionAccuracy / 100f);

        if (UnityEngine.Random.value > failChance)
        {
            return action;
        }

        BattleAction[] actions =
        {
            BattleAction.MoveAway,
            BattleAction.MoveTowards,
            BattleAction.Attack,
            BattleAction.None
        };

        return actions[UnityEngine.Random.Range(0, actions.Length)];
    }
}


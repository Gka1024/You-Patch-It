using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class BattleSimulator : MonoBehaviour
{
    public static BattleSimulator Instance;
    public PlayerProfile sampleProfile;

    public StatisticsManager statisticsManager;

    private const float TICK = 0.05f;
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
            int battleSeed = random.Next();
            results.Add(StartSimulation(match, battleSeed));
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

        BattleAIState redAI = new BattleAIState(redCharacter.OriginCharacter.battleAI, redPlayer, battleRandom);

        BattleAIState blueAI = new BattleAIState(blueCharacter.OriginCharacter.battleAI, bluePlayer, battleRandom);

        BattleCharacter red = new BattleCharacter(redCharacter, redPlayer, redAI, 0f);

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

            // 체력 회복


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
            BattleActionExecutor.ExecuteAction(red, blue, redAction, TICK, battleRandom);
            BattleActionExecutor.ExecuteAction(blue, red, blueAction, TICK, battleRandom);

            battleTime += TICK;

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

        return new BattleResult(redPlayer, bluePlayer, winner, loser, statistics);
    }

    private void TickCharacter(BattleCharacter character)
    {
        character.attackCooldown = Mathf.Max(0, character.attackCooldown - TICK);
        character.actionLockTime = Mathf.Max(0, character.actionLockTime - TICK);
        character.reactionTimer = Mathf.Max(0, character.reactionTimer - TICK);

        HealCharacter(character, character.runtimeCharacter.GetStat(CharacterStatType.HealthRegen) / 100f);
        RegenManaOnTick(character, character.runtimeCharacter.GetStat(CharacterStatType.GainMana) / 100f);

        if (character.isSkillReady)
        {
            character.skillDelayTimer = Mathf.Max(0, character.skillDelayTimer - TICK);
        }

        UpdateSkillReady(character);
    }

    private void UpdateSkillReady(BattleCharacter character)
    {
        if (character.isSkillReady) return;

        if (character.currentMana < character.maxMana) return;

        character.isSkillReady = true;
    }

    private void HealCharacter(BattleCharacter character, float amount)
    {
        character.currentHealth = Math.Min(character.currentHealth + amount, character.runtimeCharacter.GetStat(CharacterStatType.Health));
    }

    private void RegenManaOnTick(BattleCharacter character, float amount)
    {
        character.currentMana = Math.Min(character.currentMana + amount, character.maxMana);
    }

    private float GetReactionTime(BattleCharacter character)
    {
        float multiplier = Mathf.Lerp(1.4f, 0.6f, character.player.ReactionTime / 100f);

        return character.aiState.ReactionTime * multiplier;
    }


}


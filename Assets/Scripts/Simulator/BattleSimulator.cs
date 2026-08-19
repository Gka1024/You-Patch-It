using System;
using System.Collections.Generic;
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

        return Simulate(data.redCharacters, data.redPlayers, data.blueCharacters, data.bluePlayers, battleSeed);
    }

    public BattleResult Simulate(RuntimeCharacter redcharacter, RuntimePlayer redplayer, RuntimeCharacter bluecharacter, RuntimePlayer blueplayer, int battleSeed)
    {
        List<RuntimeCharacter> redCs = new();
        List<RuntimePlayer> redPs = new();
        List<RuntimeCharacter> blueCs = new();
        List<RuntimePlayer> bluePs = new();

        redCs.Add(redcharacter);
        redPs.Add(redplayer);
        blueCs.Add(bluecharacter);
        bluePs.Add(blueplayer);

        return Simulate(redCs, redPs, blueCs, bluePs, battleSeed);
    }

    public BattleResult Simulate(List<RuntimeCharacter> redCharacters, List<RuntimePlayer> redPlayers, List<RuntimeCharacter> blueCharacters, List<RuntimePlayer> bluePlayers, int battleSeed)
    {
        System.Random battleRandom = new System.Random(battleSeed);
        BattleStatistics statistics = new BattleStatistics();

        List<BattleCharacter> redTeam = CreateTeam(redCharacters, redPlayers, battleRandom, 0f);
        List<BattleCharacter> blueTeam = CreateTeam(blueCharacters, bluePlayers, battleRandom, 10f);

        RegisterStatistics(statistics, redTeam, blueTeam);

        float battleTime = 0f;

        while (IsTeamAlive(redTeam) && IsTeamAlive(blueTeam))
        {
            TickTeam(redTeam);
            TickTeam(blueTeam);

            List<BattleActionCommand> commands = new();

            CollectTeamActions(redTeam, blueTeam, commands);
            CollectTeamActions(blueTeam, redTeam, commands);

            ExecuteActions(commands, battleRandom);

            battleTime += TICK;

            if (battleTime > BATTLE_TIME_LIMIT)
            {
                Debug.LogWarning($"Battle Timeout (Seed : {battleSeed})");
                break;
            }
        }

        statistics.battleDuration = battleTime;

        UpdateSurvivalTime(redTeam, battleTime);
        UpdateSurvivalTime(blueTeam, battleTime);

        bool redAlive = IsTeamAlive(redTeam);
        bool blueAlive = IsTeamAlive(blueTeam);

        List<RuntimeCharacter> winner = new();
        List<RuntimeCharacter> loser = new();

        BattleResult result = new BattleResult(redPlayers, bluePlayers, winner, loser, statistics);

        if (redAlive && !blueAlive)
        {
            winner.AddRange(redCharacters);
            loser.AddRange(blueCharacters);
        }
        else if (!redAlive && blueAlive)
        {
            winner.AddRange(blueCharacters);
            loser.AddRange(redCharacters);
        }
        else
        {
            winner.AddRange(redCharacters);
            winner.AddRange(blueCharacters);

            result.isDraw = true;
        }

        result.isDraw = redAlive && blueAlive;
        result.battleTime = battleTime;

        return result;
    }

    private List<BattleCharacter> CreateTeam(List<RuntimeCharacter> characters, List<RuntimePlayer> players, System.Random random, float startingPosition)
    {
        List<BattleCharacter> team = new();

        if (characters == null || players == null || characters.Count != players.Count)
        {
            Debug.LogError("Character와 Player의 개수가 일치하지 않습니다.");
            return team;
        }

        for (int i = 0; i < characters.Count; i++)
        {
            RuntimeCharacter runtimeCharacter = characters[i];
            RuntimePlayer runtimePlayer = players[i];

            if (runtimeCharacter == null || runtimePlayer == null)
                continue;

            BattleAIState aiState = new BattleAIState(runtimeCharacter.OriginCharacter.battleAI, runtimePlayer, random);
            BattleCharacter character = new BattleCharacter(runtimeCharacter, runtimePlayer, aiState, startingPosition);

            team.Add(character);
        }

        return team;
    }

    private List<RuntimeCharacter> GetAliveCharacters(List<BattleCharacter> team)
    {
        List<RuntimeCharacter> characters = new();

        foreach (BattleCharacter character in team)
        {
            if (!character.IsDead)
                characters.Add(character.runtimeCharacter);
        }

        return characters;
    }

    private List<RuntimeCharacter> GetDeadCharacters(List<BattleCharacter> team)
    {
        List<RuntimeCharacter> characters = new();

        foreach (BattleCharacter character in team)
        {
            if (character.IsDead)
                characters.Add(character.runtimeCharacter);
        }

        return characters;
    }

    private void RegisterStatistics(BattleStatistics statistics, List<BattleCharacter> redTeam, List<BattleCharacter> blueTeam)
    {
        foreach (BattleCharacter character in redTeam)
            statistics.RegisterRed(character.statistics);

        foreach (BattleCharacter character in blueTeam)
            statistics.RegisterBlue(character.statistics);
    }

    private void TickTeam(List<BattleCharacter> team)
    {
        foreach (BattleCharacter character in team)
        {
            if (character.IsDead)
                continue;

            TickCharacter(character);
        }
    }

    private void TickCharacter(BattleCharacter character)
    {
        character.attackCooldown = Mathf.Max(0f, character.attackCooldown - TICK);
        character.actionLockTime = Mathf.Max(0f, character.actionLockTime - TICK);
        character.reactionTimer = Mathf.Max(0f, character.reactionTimer - TICK);

        HealCharacter(character, character.runtimeCharacter.GetStat(CharacterStatType.HealthRegen) / 100f);
        RegenManaOnTick(character, character.runtimeCharacter.GetStat(CharacterStatType.GainMana) / 100f);

        if (character.isSkillReady)
            character.skillDelayTimer = Mathf.Max(0f, character.skillDelayTimer - TICK);

        UpdateSkillReady(character);
    }

    private void UpdateSkillReady(BattleCharacter character)
    {
        if (character.isSkillReady)
            return;

        if (character.currentMana < character.GetStat(CharacterStatType.MaxMana))
            return;

        character.isSkillReady = true;
    }

    private void HealCharacter(BattleCharacter character, float amount)
    {
        float maxHealth = character.runtimeCharacter.GetStat(CharacterStatType.Health);
        character.currentHealth = Math.Min(character.currentHealth + amount, maxHealth);
    }

    private void RegenManaOnTick(BattleCharacter character, float amount)
    {
        float maxMana = character.GetStat(CharacterStatType.MaxMana);
        character.currentMana = Math.Min(character.currentMana + amount, maxMana);
    }

    private void CollectTeamActions(List<BattleCharacter> team, List<BattleCharacter> enemyTeam, List<BattleActionCommand> commands)
    {
        foreach (BattleCharacter character in team)
        {
            if (character.IsDead || !character.CanAct || !character.CanThink)
                continue;

            BattleCharacter target = SelectTarget(character, enemyTeam);

            if (target == null)
                continue;

            character.reactionTimer = GetReactionTime(character);

            BattleAction action = character.aiState.DecideAction(character, target);

            if (action == BattleAction.None)
                continue;

            commands.Add(new BattleActionCommand(character, target, action));
        }
    }

    private BattleCharacter SelectTarget(BattleCharacter attacker, List<BattleCharacter> enemyTeam)
    {
        BattleCharacter target = null;
        float closestDistance = float.MaxValue;

        foreach (BattleCharacter enemy in enemyTeam)
        {
            if (enemy.IsDead)
                continue;

            float distance = GetDistance(attacker, enemy);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = enemy;
            }
        }

        return target;
    }

    private float GetDistance(BattleCharacter attacker, BattleCharacter target)
    {
        return Mathf.Abs(attacker.position - target.position);
    }

    private void ExecuteActions(List<BattleActionCommand> commands, System.Random random)
    {
        foreach (BattleActionCommand command in commands)
        {
            if (command.attacker == null || command.attacker.IsDead)
                continue;

            if (command.target == null || command.target.IsDead)
                continue;

            BattleActionExecutor.ExecuteAction(command.attacker, command.target, command.action, TICK, random);
        }
    }

    private bool IsTeamAlive(List<BattleCharacter> team)
    {
        foreach (BattleCharacter character in team)
        {
            if (!character.IsDead)
                return true;
        }

        return false;
    }

    private void UpdateSurvivalTime(List<BattleCharacter> team, float battleTime)
    {
        foreach (BattleCharacter character in team)
            character.statistics.survivalTime = battleTime;
    }

    private float GetReactionTime(BattleCharacter character)
    {
        float multiplier = Mathf.Lerp(1.4f, 0.6f, character.player.ReactionTime / 100f);
        return character.aiState.ReactionTime * multiplier;
    }
}

public class BattleActionCommand
{
    public BattleCharacter attacker;
    public BattleCharacter target;
    public BattleAction action;

    public BattleActionCommand(BattleCharacter attacker, BattleCharacter target, BattleAction action)
    {
        this.attacker = attacker;
        this.target = target;
        this.action = action;
    }
}
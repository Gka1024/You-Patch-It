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

    private readonly List<BattleCharacter> redTeam = new();
    private readonly List<BattleCharacter> blueTeam = new();
    private readonly List<BattleActionCommand> commands = new();
    private readonly Stack<BattleActionCommand> commandPool = new();
    private readonly Stack<BattleCharacter> characterPool = new();

    public int CommandPoolCount => commandPool.Count;
    public int CharacterPoolCount => characterPool.Count;

    private void Awake()
    {
        Instance = this;
    }

    public List<BattleResult> StartSimulation(List<MatchData> matches, System.Random random)
    {
        List<BattleResult> results = new(matches.Count);

        foreach (MatchData match in matches)
        {
            int battleSeed = random.Next();
            results.Add(StartSimulation(match, battleSeed));
        }

        return results;
    }

    public BattleResult StartSimulation(MatchData data, int simulationSeed)
    {
        System.Random simulationRandom = new(simulationSeed);
        int battleSeed = simulationRandom.Next();

        return Simulate(data.redCharacters, data.redPlayers, data.blueCharacters, data.bluePlayers, battleSeed);
    }

    public BattleResult Simulate(RuntimeCharacter redCharacter, RuntimePlayer redPlayer, RuntimeCharacter blueCharacter, RuntimePlayer bluePlayer, int battleSeed)
    {
        List<RuntimeCharacter> redCharacters = new() { redCharacter };
        List<RuntimePlayer> redPlayers = new() { redPlayer };
        List<RuntimeCharacter> blueCharacters = new() { blueCharacter };
        List<RuntimePlayer> bluePlayers = new() { bluePlayer };

        return Simulate(redCharacters, redPlayers, blueCharacters, bluePlayers, battleSeed);
    }

    public BattleResult Simulate(List<RuntimeCharacter> redCharacters, List<RuntimePlayer> redPlayers, List<RuntimeCharacter> blueCharacters, List<RuntimePlayer> bluePlayers, int battleSeed)
    {
        System.Random battleRandom = new(battleSeed);
        BattleStatistics statistics = new();

        PrepareTeams(redCharacters, redPlayers, blueCharacters, bluePlayers, battleRandom);
        RegisterStatistics(statistics);

        float battleTime = 0f;

        while (IsTeamAlive(redTeam) && IsTeamAlive(blueTeam))
        {
            TickTeam(redTeam);
            TickTeam(blueTeam);

            commands.Clear();

            CollectTeamActions(redTeam, blueTeam);
            CollectTeamActions(blueTeam, redTeam);

            ExecuteActions(battleRandom);

            battleTime += TICK;

            if (battleTime > BATTLE_TIME_LIMIT)
            {
                Debug.LogWarning($"Battle Timeout (Seed : {battleSeed})");
                break;
            }
        }

        statistics.battleDuration = battleTime;

        bool redAlive = IsTeamAlive(redTeam);
        bool blueAlive = IsTeamAlive(blueTeam);

        List<RuntimeCharacter> winner = new();
        List<RuntimeCharacter> loser = new();

        bool isDraw = false;

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
            isDraw = true;
        }

        BattleResult result = new BattleResult(redPlayers, bluePlayers, winner, loser, statistics);
        result.isDraw = isDraw;
        result.battleTime = battleTime;

        ReleaseCommands();
        ReleaseTeams();

        return result;
    }

    private void PrepareTeams(List<RuntimeCharacter> redCharacters, List<RuntimePlayer> redPlayers, List<RuntimeCharacter> blueCharacters, List<RuntimePlayer> bluePlayers, System.Random random)
    {
        redTeam.Clear();
        blueTeam.Clear();

        CreateTeam(redCharacters, redPlayers, random, 0f, redTeam);
        CreateTeam(blueCharacters, bluePlayers, random, 10f, blueTeam);
    }

    private void CreateTeam(List<RuntimeCharacter> characters, List<RuntimePlayer> players, System.Random random, float startingPosition, List<BattleCharacter> team)
    {
        if (characters == null || players == null || characters.Count != players.Count)
        {
            Debug.LogError("Character와 Player의 개수가 일치하지 않습니다.");
            return;
        }

        for (int i = 0; i < characters.Count; i++)
        {
            RuntimeCharacter runtimeCharacter = characters[i];
            RuntimePlayer runtimePlayer = players[i];

            if (runtimeCharacter == null || runtimePlayer == null)
                continue;

            BattleAIState aiState = new BattleAIState(runtimeCharacter.OriginCharacter.battleAI, runtimePlayer, random);
            BattleCharacter character = GetBattleCharacter();

            character.Initialize(runtimeCharacter, runtimePlayer, aiState, startingPosition);
            team.Add(character);
        }
    }

    private BattleCharacter GetBattleCharacter()
    {
        if (characterPool.Count > 0)
            return characterPool.Pop();

        return new BattleCharacter();
    }

    private void ReleaseTeams()
    {
        ReleaseTeam(redTeam);
        ReleaseTeam(blueTeam);

        redTeam.Clear();
        blueTeam.Clear();
    }

    private void ReleaseTeam(List<BattleCharacter> team)
    {
        for (int i = 0; i < team.Count; i++)
        {
            BattleCharacter character = team[i];
            character.Reset();
            characterPool.Push(character);
        }
    }

    private void RegisterStatistics(BattleStatistics statistics)
    {
        for (int i = 0; i < redTeam.Count; i++)
            statistics.RegisterRed(redTeam[i].statistics);

        for (int i = 0; i < blueTeam.Count; i++)
            statistics.RegisterBlue(blueTeam[i].statistics);
    }

    private void TickTeam(List<BattleCharacter> team)
    {
        for (int i = 0; i < team.Count; i++)
        {
            BattleCharacter character = team[i];

            if (character.IsDead)
                continue;

            character.statistics.survivalTime += TICK;
            TickCharacter(character);
        }
    }

    private void TickCharacter(BattleCharacter character)
    {
        character.attackCooldown = Mathf.Max(0f, character.attackCooldown - TICK);
        character.actionLockTime = Mathf.Max(0f, character.actionLockTime - TICK);
        character.reactionTimer = Mathf.Max(0f, character.reactionTimer - TICK);
        character.targetUpdateTimer = Mathf.Max(0f, character.targetUpdateTimer - TICK);

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

    private void CollectTeamActions(List<BattleCharacter> team, List<BattleCharacter> enemyTeam)
    {
        for (int i = 0; i < team.Count; i++)
        {
            BattleCharacter character = team[i];

            if (character.IsDead || !character.CanAct || !character.CanThink)
                continue;

            UpdateTarget(character, enemyTeam);

            if (character.currentTarget == null)
                continue;

            character.reactionTimer = GetReactionTime(character);

            BattleAction action = character.aiState.DecideAction(character, character.currentTarget);

            if (action == BattleAction.None)
                continue;

            BattleActionCommand command = GetCommand();
            command.Set(character, character.currentTarget, action);
            commands.Add(command);
        }
    }

    private void UpdateTarget(BattleCharacter character, List<BattleCharacter> enemyTeam)
    {
        if (character.currentTarget == null || character.currentTarget.IsDead)
        {
            character.currentTarget = SelectTarget(character, enemyTeam);
            character.targetUpdateTimer = GetTargetUpdateInterval(character);
            return;
        }

        if (character.targetUpdateTimer > 0f)
            return;

        character.currentTarget = SelectTarget(character, enemyTeam);
        character.targetUpdateTimer = GetTargetUpdateInterval(character);
    }

    private float GetTargetUpdateInterval(BattleCharacter character)
    {
        float reaction = character.player.ReactionTime / 100f;
        return Mathf.Lerp(1f, 0.5f, reaction);
    }

    private BattleActionCommand GetCommand()
    {
        if (commandPool.Count > 0)
            return commandPool.Pop();

        return new BattleActionCommand();
    }

    private void ReleaseCommands()
    {
        for (int i = 0; i < commands.Count; i++)
        {
            BattleActionCommand command = commands[i];
            command.Clear();
            commandPool.Push(command);
        }

        commands.Clear();
    }

    private BattleCharacter SelectTarget(BattleCharacter attacker, List<BattleCharacter> enemyTeam)
    {
        return attacker.aiState.GetTarget(attacker, enemyTeam);
    }

    private void ExecuteActions(System.Random random)
    {
        for (int i = 0; i < commands.Count; i++)
        {
            BattleActionCommand command = commands[i];

            if (command.attacker == null || command.attacker.IsDead)
                continue;

            if (command.target == null || command.target.IsDead)
                continue;

            BattleActionExecutor.ExecuteAction(command.attacker, command.target, command.action, TICK, random);
        }
    }

    private bool IsTeamAlive(List<BattleCharacter> team)
    {
        for (int i = 0; i < team.Count; i++)
        {
            if (!team[i].IsDead)
                return true;
        }

        return false;
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

    public BattleActionCommand()
    {
    }

    public void Set(BattleCharacter attacker, BattleCharacter target, BattleAction action)
    {
        this.attacker = attacker;
        this.target = target;
        this.action = action;
    }

    public void Clear()
    {
        attacker = null;
        target = null;
        action = BattleAction.None;
    }
}
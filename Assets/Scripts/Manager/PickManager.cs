using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PickManager : MonoBehaviour
{
    public static PickManager Instance;

    private int TeamSize;

    private const int TEAM_SIZE_3 = 3051;

    private const float MIN_META_LEARNING_RATE = 0.03f;
    private const float MAX_META_LEARNING_RATE = 0.25f;

    private void Awake()
    {
        TeamSize = 1;
        Instance = this;
    }

    private void Start()
    {
        UnlockManager.Instance.OnUnlockChanged += CheckTeamSize;
        CheckTeamSize();
    }

    private void OnDestroy()
    {
        if (UnlockManager.Instance != null)
            UnlockManager.Instance.OnUnlockChanged -= CheckTeamSize;

        if (Instance == this)
            Instance = null;
    }

    //=========================================================
    // Match Making
    //=========================================================

    private void CheckTeamSize()
    {
        if (UnlockManager.Instance.IsUnlocked(TEAM_SIZE_3))
        {
            UnlockManager.Instance.OnUnlockChanged -= CheckTeamSize;
            TeamSize = 3;
            AnalysisManager.Instance.SetCurrentTeamSize(TeamSize);
        }
    }

    public List<MatchData> StartPick(IReadOnlyList<RuntimePlayer> players, System.Random random)
    {
        Dictionary<PlayerTier, Queue<RuntimePlayer>> queues = CreateQueues(players, random);
        List<MatchData> matches = new();

        while (true)
        {
            List<RuntimePlayer> redPlayers = GetNextTeam(queues);

            if (redPlayers == null)
                break;

            List<RuntimePlayer> bluePlayers = FindOpponentTeam(redPlayers[0], queues);

            if (bluePlayers == null)
            {
                // 상대를 찾지 못한 플레이어를 다시 큐에 넣어 유실을 방지한다.
                foreach (RuntimePlayer player in redPlayers)
                    queues[player.Tier].Enqueue(player);

                break;
            }

            List<RuntimeCharacter> redCharacters = PickTeamCharacters(redPlayers, random);
            List<RuntimeCharacter> blueCharacters = PickTeamCharacters(bluePlayers, random);

            matches.Add(new MatchData(redPlayers, bluePlayers, redCharacters, blueCharacters));
        }

        return matches;
    }

    public List<MatchData> StartPick(IReadOnlyList<RuntimePlayer> players, System.Random random, List<RuntimeCharacter> baseCharacters, List<RuntimeCharacter> opponentCharacters)
    {
        Dictionary<PlayerTier, Queue<RuntimePlayer>> queues = CreateQueues(players, random);
        List<MatchData> matches = new();

        while (true)
        {
            List<RuntimePlayer> redPlayers = GetNextTeam(queues);

            if (redPlayers == null)
                break;

            List<RuntimePlayer> bluePlayers = FindOpponentTeam(redPlayers[0], queues);

            if (bluePlayers == null)
            {
                foreach (RuntimePlayer player in redPlayers)
                    queues[player.Tier].Enqueue(player);

                break;
            }

            List<RuntimeCharacter> redCharacters = new(baseCharacters);
            List<RuntimeCharacter> blueCharacters = new(opponentCharacters);

            matches.Add(new MatchData(redPlayers, bluePlayers, redCharacters, blueCharacters));
        }

        return matches;
    }

    private Dictionary<PlayerTier, Queue<RuntimePlayer>> CreateQueues(IReadOnlyList<RuntimePlayer> players, System.Random random)
    {
        Dictionary<PlayerTier, Queue<RuntimePlayer>> queues = new();

        foreach (PlayerTier tier in Enum.GetValues(typeof(PlayerTier)))
            queues[tier] = new Queue<RuntimePlayer>();

        foreach (RuntimePlayer player in players.OrderBy(_ => random.Next()))
            queues[player.Tier].Enqueue(player);

        return queues;
    }

    private List<RuntimePlayer> GetNextTeam(Dictionary<PlayerTier, Queue<RuntimePlayer>> queues)
    {
        foreach (Queue<RuntimePlayer> queue in queues.Values)
        {
            if (queue.Count < TeamSize)
                continue;

            List<RuntimePlayer> team = new();

            for (int i = 0; i < TeamSize; i++)
                team.Add(queue.Dequeue());

            return team;
        }

        return null;
    }

    private List<RuntimePlayer> FindOpponentTeam(RuntimePlayer player, Dictionary<PlayerTier, Queue<RuntimePlayer>> queues)
    {
        int tier = (int)player.Tier;
        int tierCount = Enum.GetValues(typeof(PlayerTier)).Length;

        List<PlayerTier> searchTiers = new() { player.Tier };

        if (tier > 0)
            searchTiers.Add((PlayerTier)(tier - 1));

        if (tier < tierCount - 1)
            searchTiers.Add((PlayerTier)(tier + 1));

        foreach (PlayerTier searchTier in searchTiers)
        {
            if (queues[searchTier].Count < TeamSize)
                continue;

            List<RuntimePlayer> team = new();

            for (int i = 0; i < TeamSize; i++)
                team.Add(queues[searchTier].Dequeue());

            return team;
        }

        return null;
    }

    //=========================================================
    // Pick
    //=========================================================

    private List<RuntimeCharacter> PickTeamCharacters(List<RuntimePlayer> players, System.Random random)
    {
        List<RuntimeCharacter> characters = new();

        foreach (RuntimePlayer player in players)
        {
            RuntimeCharacter character = PickCharacter(player, random, characters);

            if (character != null)
                characters.Add(character);
        }

        return characters;
    }

    public RuntimeCharacter PickCharacter(RuntimePlayer player, System.Random random)
    {
        return PickCharacter(player, random, new List<RuntimeCharacter>());
    }

    private RuntimeCharacter PickCharacter(RuntimePlayer player, System.Random random, List<RuntimeCharacter> pickedCharacters)
    {
        List<RuntimeCharacter> characters = RuntimeCharacterManager.Instance.GetAllCharacters()
            .Where(character => !pickedCharacters.Contains(character))
            .ToList();

        if (characters.Count == 0)
            return null;

        List<float> scores = new();
        float totalScore = 0f;

        foreach (RuntimeCharacter character in characters)
        {
            float score = Mathf.Max(1f, GetPickScore(character, player));

            scores.Add(score);
            totalScore += score;
        }

        double roll = random.NextDouble() * totalScore;
        float accumulated = 0f;

        for (int i = 0; i < characters.Count; i++)
        {
            accumulated += scores[i];

            if (roll <= accumulated)
                return characters[i];
        }

        return characters[^1];
    }

    public float GetPickScore(RuntimeCharacter character, RuntimePlayer player)
    {
        float score = 50f;

        score += WinrateScore(character, player);
        score += PickRateScore(character, player);
        score += PreferenceScore(character, player);

        return Mathf.Max(1f, score);
    }

    //---------------------------------------------------------
    // Meta Knowledge
    //---------------------------------------------------------

    public void UpdatePlayerMetaKnowledge(IReadOnlyList<RuntimePlayer> players)
    {
        List<RuntimeCharacter> characters = RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        foreach (RuntimePlayer player in players)
        {
            float learningRate = Mathf.Lerp(
                MIN_META_LEARNING_RATE,
                MAX_META_LEARNING_RATE,
                Mathf.Clamp01(player.MetaKnowledge / 100f));

            foreach (RuntimeCharacter character in characters)
            {
                float observedPickRate = AnalysisManager.Instance.GetPickRate(character);

                if (!player.KnownPickRates.TryGetValue(character, out float knownPickRate))
                {
                    player.KnownPickRates[character] = observedPickRate;
                    continue;
                }

                player.KnownPickRates[character] = Mathf.Lerp(
                    knownPickRate,
                    observedPickRate,
                    learningRate);
            }
        }
    }

    //---------------------------------------------------------
    // Pick Score
    //---------------------------------------------------------

    private float WinrateScore(RuntimeCharacter character, RuntimePlayer player)
    {
        float winRate = StatisticsManager.Instance.GetCurrentStatistics(character).Winrate;
        float delta = winRate - 50f;

        float experimentWeight = 1f - player.RiskTaking / 200f;

        return delta * 2f * (player.MetaKnowledge / 100f) * experimentWeight;
    }

    private float PickRateScore(RuntimeCharacter character, RuntimePlayer player)
    {
        if (!player.KnownPickRates.TryGetValue(character, out float knownPickRate))
            knownPickRate = AnalysisManager.Instance.GetPickRate(character);

        return knownPickRate * 0.3f;
    }

    private float PreferenceScore(RuntimeCharacter character, RuntimePlayer player)
    {
        if (!player.ClassPreferences.TryGetValue(character.OriginCharacter.role, out float preference))
            return 0f;

        float weight = (100f - player.MetaDependence) / 100f;

        return (preference - 30f) * weight;
    }
}
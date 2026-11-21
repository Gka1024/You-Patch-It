
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PickManager : MonoBehaviour
{
    public static PickManager Instance;

    private const int TEAM_SIZE_3 = 3051;

    private const float MIN_META_LEARNING_RATE = 0.6f;
    private const float MAX_META_LEARNING_RATE = 0.9f;

    [Header("Team Settings")]
    private int TeamSize = 3;

    [Header("Pick Score Weights")]
    [SerializeField][Range(1, 10)] private float winrateWeight = 1f;
    [SerializeField][Range(1, 10)] private float pickrateWeight = 1f;
    [SerializeField][Range(1, 10)] private float roleWeight = 1f;
    [SerializeField][Range(1, 10)] private float characterWeight = 1f;

    private readonly Dictionary<RuntimeCharacter, float> characterPreferenceDictionary = new();
    private readonly Dictionary<RuntimeCharacter, float> sumPickScore = new();
    private readonly Dictionary<RuntimeCharacter, int> timePicked = new();

    // =========================================================
    // Lifecycle
    // =========================================================

    private void Awake()
    {
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

    // =========================================================
    // Team Settings
    // =========================================================

    private void CheckTeamSize()
    {
        if (!UnlockManager.Instance.IsUnlocked(TEAM_SIZE_3))
            return;

        UnlockManager.Instance.OnUnlockChanged -= CheckTeamSize;

        TeamSize = 3;
        AnalysisManager.Instance.SetCurrentTeamSize(TeamSize);
    }

    // =========================================================
    // Match Making & Pick
    // =========================================================

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
                ReturnPlayersToQueues(redPlayers, queues);
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
                ReturnPlayersToQueues(redPlayers, queues);
                break;
            }

            List<RuntimeCharacter> redCharacters = new(baseCharacters);
            List<RuntimeCharacter> blueCharacters = new(opponentCharacters);

            matches.Add(new MatchData(redPlayers, bluePlayers, redCharacters, blueCharacters));
        }

        return matches;
    }

    private void ReturnPlayersToQueues(List<RuntimePlayer> players, Dictionary<PlayerTier, Queue<RuntimePlayer>> queues)
    {
        foreach (RuntimePlayer player in players)
            queues[player.Tier].Enqueue(player);
    }

    // =========================================================
    // Queue Management
    // =========================================================

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

            return DequeueTeam(queue);
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
            Queue<RuntimePlayer> queue = queues[searchTier];

            if (queue.Count < TeamSize)
                continue;

            return DequeueTeam(queue);
        }

        return null;
    }

    private List<RuntimePlayer> DequeueTeam(Queue<RuntimePlayer> queue)
    {
        List<RuntimePlayer> team = new(TeamSize);

        for (int i = 0; i < TeamSize; i++)
            team.Add(queue.Dequeue());

        return team;
    }

    // =========================================================
    // Character Selection
    // =========================================================

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

    // =========================================================
    // Pick Score
    // =========================================================

    public float GetPickScore(RuntimeCharacter character, RuntimePlayer player)
    {
        float score = 50f;

        score += WinrateScore(character, player) * winrateWeight;
        score += PickRateScore(character, player) * pickrateWeight;
        score += GetRolePreferenceScore(character, player) * roleWeight;
        score += GetCharacterPreference(character) * characterWeight;

        RecordPickScore(character, score);

        return Mathf.Max(1f, score);
    }

    private void RecordPickScore(RuntimeCharacter character, float score)
    {
        timePicked[character] = timePicked.GetValueOrDefault(character) + 1;
        sumPickScore[character] = sumPickScore.GetValueOrDefault(character) + score;
    }

    public void GetAveragePickScore()
    {
        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            Debug.Log($"{character.OriginCharacter.characterName} : {sumPickScore[character] / timePicked[character]:F1}");
        }
    }

    private float WinrateScore(RuntimeCharacter character, RuntimePlayer player)
    {
        float winRate = StatisticsManager.Instance.GetCurrentStatistics(character).Winrate;

        if (winRate == 0f)
        { // 가장 처음 시작하는 캐릭터 보정
            winRate = 50f;
        }
        else if (winRate == 0f && RuntimeCharacterManager.Instance.AddedRuntimeCharacter == character)
        { // 새로 추가된 캐릭터 보정
            winRate = 70f;
        }

        float metaWeight = player.MetaKnowledge / 100f;
        float experimentWeight = 1f - player.RiskTaking / 200f;

        float delta = winRate - 50f;
        float score = delta * Mathf.Abs(delta) * 0.5f;

        return score * metaWeight * experimentWeight;
    }

    private float PickRateScore(RuntimeCharacter character, RuntimePlayer player)
    {
        if (!player.KnownPickRates.TryGetValue(character, out float knownPickRate))
        {
            knownPickRate = AnalysisManager.Instance.GetPickRate(character);
            knownPickRate = knownPickRate == 0 ? 100f / RuntimeCharacterManager.Instance.CharacterCount : knownPickRate;
        }

        float metaWeight = player.MetaKnowledge / 100f;

        return knownPickRate * metaWeight;
    }

    private float GetRolePreferenceScore(RuntimeCharacter character, RuntimePlayer player)
    {
        if (!player.ClassPreferences.TryGetValue(character.OriginCharacter.role, out float preference))
            return 0f;

        float weight = (100f - player.MetaDependence) / 100f;

        return (preference - 30f) * weight;
    }

    private float GetCharacterPreference(RuntimeCharacter character)
    {
        if (characterPreferenceDictionary.TryGetValue(character, out float value))
            return value;

        return 0f;
    }

    // =========================================================
    // Meta Learning
    // =========================================================

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

    public void UpdateAllCharacterPreference()
    {
        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            UpdateCharacterPreference(character, GetPreferenceDelta());
        }
    }

    public void UpdateCharacterPreference(RuntimeCharacter character, float delta)
    {
        if (characterPreferenceDictionary.TryGetValue(character, out var value))
        {
            characterPreferenceDictionary[character] = value + delta;
        }
        else
        {
            characterPreferenceDictionary.Add(character, GetRandomPreference());
        }
    }

    public float GetRandomPreference()
    {
        return UnityEngine.Random.Range(5f, 20f);
    }

    public float GetPreferenceDelta()
    {
        return UnityEngine.Random.Range(-1f, 1f);
    }

    public void GetCharacterPreferenceDebug()
    {
        foreach (var kvp in characterPreferenceDictionary)
        {
            Debug.Log($"{kvp.Key.OriginCharacter.characterName} : {kvp.Value:F1} ");
        }
    }
}

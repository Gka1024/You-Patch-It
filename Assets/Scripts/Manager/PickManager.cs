using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PickManager : MonoBehaviour
{
    public static PickManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    //=========================================================
    // Match Making
    //=========================================================

    public List<MatchData> StartPick(IReadOnlyList<RuntimePlayer> players, System.Random random)
    {
        Dictionary<PlayerTier, Queue<RuntimePlayer>> queues = CreateQueues(players, random);

        List<MatchData> matches = new();

        while (true)
        {
            RuntimePlayer player = GetNextPlayer(queues);

            if (player == null)
                break;

            RuntimePlayer opponent = FindOpponent(player, queues);

            if (opponent == null)
                continue;

            RuntimeCharacter redCharacter = PickCharacter(player, random);
            RuntimeCharacter blueCharacter = PickCharacter(opponent, random);

            matches.Add(new MatchData(
                player,
                opponent,
                redCharacter,
                blueCharacter));
        }

        return matches;
    }

    private Dictionary<PlayerTier, Queue<RuntimePlayer>> CreateQueues(IReadOnlyList<RuntimePlayer> players, System.Random random)
    {
        Dictionary<PlayerTier, Queue<RuntimePlayer>> queues = new();

        foreach (PlayerTier tier in Enum.GetValues(typeof(PlayerTier)))
        {
            queues[tier] = new Queue<RuntimePlayer>();
        }

        foreach (RuntimePlayer player in players.OrderBy(_ => random.Next()))
        {
            queues[player.Tier].Enqueue(player);
        }

        return queues;
    }

    private RuntimePlayer GetNextPlayer(Dictionary<PlayerTier, Queue<RuntimePlayer>> queues)
    {
        foreach (Queue<RuntimePlayer> queue in queues.Values)
        {
            if (queue.Count > 0)
                return queue.Dequeue();
        }

        return null;
    }

    private RuntimePlayer FindOpponent(RuntimePlayer player,
        Dictionary<PlayerTier, Queue<RuntimePlayer>> queues)
    {
        int tier = (int)player.Tier;

        // 같은 티어
        if (queues[player.Tier].Count > 0)
            return queues[player.Tier].Dequeue();

        // 아래 티어
        if (tier > 0)
        {
            PlayerTier lower = (PlayerTier)(tier - 1);

            if (queues[lower].Count > 0)
                return queues[lower].Dequeue();
        }

        // 위 티어
        if (tier < Enum.GetValues(typeof(PlayerTier)).Length - 1)
        {
            PlayerTier upper = (PlayerTier)(tier + 1);

            if (queues[upper].Count > 0)
                return queues[upper].Dequeue();
        }

        return null;
    }

    //=========================================================
    // Pick
    //=========================================================

    public RuntimeCharacter PickCharacter(RuntimePlayer player, System.Random random)
    {
        List<RuntimeCharacter> characters =
            RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        List<float> scores = new();

        float totalScore = 0f;

        foreach (RuntimeCharacter character in characters)
        {
            float score = Mathf.Max(1f, GetPickScore(character, player));

            scores.Add(score);
            totalScore += score;
        }

        double roll = random.NextDouble() * totalScore;

        float accumulated = 0;

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
        float score = 50;

        score += WinrateScore(character, player);
        score += PickRateScore(character);
        score += PreferenceScore(character, player);

        return Mathf.Max(1, score);
    }

    //---------------------------------------------------------

    private float WinrateScore(RuntimeCharacter character, RuntimePlayer player)
    {
        float winRate =
            StatisticsManager.Instance.GetCurrentStatistics(character).WinRate;

        float delta = winRate - 50f;

        float experimentWeight =
            1f - player.experiment / 200f;

        return delta
             * 2f
             * (player.metaKnowledge / 100f)
             * experimentWeight;
    }

    private float PickRateScore(RuntimeCharacter character)
    {
        float pickRate =
            AnalysisManager.Instance.GetPickRate(character);

        return pickRate * 0.3f ;
    }

    private float PreferenceScore(RuntimeCharacter character, RuntimePlayer player)
    {
        if (!player.classPreferences.TryGetValue(
            character.OriginCharacter.role,
            out float preference))
            return 0;

        float weight =
            (100f - player.metaDependence) / 100f;

        return (preference - 30f) * weight;
    }
}
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PickManager : MonoBehaviour
{
    public static PickManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public List<MatchData> StartPick(IReadOnlyList<RuntimePlayer> players, System.Random random)
    {
        List<MatchData> Matches = new();

        for (int i = 0; i < players.Count; i += 2)
        {
            RuntimePlayer redPlayer = players[i];
            RuntimePlayer bluePlayer = players[i + 1];

            RuntimeCharacter redCharacter = PickCharacter(redPlayer.originalProfile, random);
            RuntimeCharacter blueCharacter = PickCharacter(bluePlayer.originalProfile, random);

            Matches.Add(new MatchData(redPlayer, bluePlayer, redCharacter, blueCharacter));
        }

        return Matches;
    }

    public RuntimeCharacter PickCharacter(PlayerProfile profile, System.Random random)
    {
        List<RuntimeCharacter> characters = RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        List<float> scores = new();

        float totalScore = 0f;

        foreach (RuntimeCharacter character in characters)
        {
            float score = Mathf.Max(1f, GetPickScore(character, profile));

            scores.Add(score);
            totalScore += score;
        }

        double roll = random.NextDouble() * totalScore;

        float accumulated = 0f;

        for (int i = 0; i < characters.Count; i++)
        {
            accumulated += scores[i];

            if (roll <= accumulated)
            {
                return characters[i];
            }
        }

        return characters[^1];
    }

    public float GetPickScore(RuntimeCharacter character, PlayerProfile profile)
    {
        float score = 50f;

        score += WinrateScore(character, profile.metaKnowledge);
        score += PickRateScore(character);
        score += PreferenceScore(character, profile);

        return Mathf.Max(1f, score);
    }

    private float WinrateScore(RuntimeCharacter character, float metaKnowledge)
    {
        float winRate = StatisticsManager.Instance.GetStatistics(character).WinRate;

        float delta = winRate - 50f;

        return delta * 2f * (metaKnowledge / 100f);
    }

    private float PickRateScore(RuntimeCharacter character)
    {
        float pickRate = StatisticsManager.Instance.GetPickRate(character);

        return pickRate * 0.3f;
    }

    private float PreferenceScore(RuntimeCharacter character, PlayerProfile profile)
    {
        return 0f;
    }
}
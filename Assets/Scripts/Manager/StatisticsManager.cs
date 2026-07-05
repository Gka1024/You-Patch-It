using System.Collections.Generic;
using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance { get; private set; }

    private Dictionary<int, CharacterStatistics> statistics = new();

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(CharacterDatabase database)
    {
        statistics.Clear();

        foreach (Character character in database.GetAllCharacters())
        {
            statistics.Add(character.id, new CharacterStatistics());
        }
    }

    public CharacterStatistics GetStatistics(int characterId)
    {
        return statistics[characterId];
    }

    public CharacterStatistics GetStatistics(RuntimeCharacter character)
    {
        return statistics[character.OriginCharacter.id];
    }

    public IReadOnlyDictionary<int, CharacterStatistics> GetAllStatistics()
    {
        return statistics;
    }

    public void RecordMatch(MatchResult result)
    {
        CharacterStatistics stat = statistics[result.CharacterID];

        stat.MatchCount++;

        if (result.IsPicked)
            stat.PickCount++;

        if (result.IsWin)
            stat.WinCount++;
        else
            stat.LoseCount++;

        stat.TotalDamage += result.DamageDealt;
        stat.TotalSurvivalTime += result.SurvivalTime;
    }

    public void ResetSeason()
    {
        foreach (CharacterStatistics stat in statistics.Values)
        {
            stat.Reset();
        }
    }
}
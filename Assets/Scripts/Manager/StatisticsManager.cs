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

        // Debug.Log($"statisticsCount : {statistics.Count}");
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

    public void RecordBattle(BattleResult result)
    {
        foreach (CharacterBattleStatistics battleStat in result.statistics.GetAll())
        {
            CharacterStatistics totalStat = GetStatistics(battleStat.runtimeCharacter);

            totalStat.MatchCount++;
            totalStat.TotalDamage += battleStat.damageDealt;
            totalStat.TotalSurvivalTime += battleStat.survivalTime;

            if (ReferenceEquals(result.winner, battleStat.runtimeCharacter)) totalStat.WinCount++;
            if (ReferenceEquals(result.loser, battleStat.runtimeCharacter)) totalStat.LoseCount++;

        }
    }

    public void ResetSeason()
    {
        foreach (CharacterStatistics stat in statistics.Values)
        {
            stat.Reset();
        }
    }
}

public class BattleStatistics
{
    public float battleDuration;

    private Dictionary<int, CharacterBattleStatistics> characterStatistics = new();

    public void Register(RuntimeCharacter runtimeCharacter)
    {
        characterStatistics.Add(runtimeCharacter.OriginCharacter.id, new CharacterBattleStatistics() { runtimeCharacter = runtimeCharacter });
    }

    public CharacterBattleStatistics Get(RuntimeCharacter runtimeCharacter)
    {
        return characterStatistics[runtimeCharacter.OriginCharacter.id];
    }

    public IEnumerable<CharacterBattleStatistics> GetAll()
    {
        return characterStatistics.Values;
    }
}


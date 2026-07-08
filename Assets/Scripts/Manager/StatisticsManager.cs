using System.Collections.Generic;
using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance { get; private set; }

    private Dictionary<int, CharacterStatistics> statistics = new();

    [SerializeField] private int totalBattles = 0;

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

    public void RecordBattle(List<BattleResult> results)
    {
        foreach (BattleResult result in results)
        {
            RecordBattle(result);
        }
    }

    public void RecordBattle(BattleResult result)
    {
        foreach (CharacterBattleStatistics battleStat in result.statistics.GetAll())
        {
            CharacterStatistics totalStat = GetStatistics(battleStat.runtimeCharacter);

            PickCount(totalStat);
            totalStat.TotalDamage += battleStat.damageDealt;
            totalStat.TotalSurvivalTime += battleStat.survivalTime;

            if (ReferenceEquals(result.winner, battleStat.runtimeCharacter)) totalStat.WinCount++;
            if (ReferenceEquals(result.loser, battleStat.runtimeCharacter)) totalStat.LoseCount++;

        }
    }

    private void PickCount(CharacterStatistics statistics)
    {
        statistics.MatchCount++;
        totalBattles++;
    }

    public float GetPickRate(RuntimeCharacter character)
    {
        if (totalBattles != 0) return (float)GetStatistics(character).MatchCount / totalBattles;
        else return 0;
    }

    public void ResetSeason()
    {
        totalBattles = 0;
        foreach (CharacterStatistics stat in statistics.Values)
        {
            stat.Reset();
        }
    }
}

public class BattleStatistics
{
    public float battleDuration;

    private readonly List<CharacterBattleStatistics> characterStatistics = new();

    public void Register(CharacterBattleStatistics statistics)
    {
        characterStatistics.Add(statistics);
    }

    public IEnumerable<CharacterBattleStatistics> GetAll()
    {
        return characterStatistics;
    }
}


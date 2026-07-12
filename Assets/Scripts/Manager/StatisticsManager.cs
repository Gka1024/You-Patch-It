using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance { get; private set; }

    private Dictionary<int, CharacterStatistics> currentStatistics = new();
    private Dictionary<int, CharacterStatistics> pastStatistics = new();

    public bool HasPastSeasonData { get; private set; }

    private Dictionary<(int, int), MatchupStatistics> currentMatchDatas = new();
    private Dictionary<(int, int), MatchupStatistics> pastMatchDatas = new();

    [SerializeField] private int totalBattles = 0;
    [SerializeField] private int pastTotalBattles = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(CharacterDatabase database)
    {
        currentStatistics.Clear();
        pastStatistics.Clear();

        currentMatchDatas.Clear();
        pastMatchDatas.Clear();

        List<Character> characters = database.GetAllCharacters().ToList();

        foreach (Character character in characters)
        {
            currentStatistics.Add(character.id, new CharacterStatistics());
            pastStatistics.Add(character.id, new CharacterStatistics());
        }

        foreach (Character baseCharacter in characters)
        {
            foreach (Character opponentCharacter in characters)
            {
                currentMatchDatas.Add(
                    (baseCharacter.id, opponentCharacter.id),
                    new MatchupStatistics());

                pastMatchDatas.Add(
                    (baseCharacter.id, opponentCharacter.id),
                    new MatchupStatistics());
            }
        }

        HasPastSeasonData = false;
    }

    public CharacterStatistics GetStatistics(int characterId)
    {
        return currentStatistics[characterId];
    }

    public CharacterStatistics GetStatistics(RuntimeCharacter character)
    {
        return currentStatistics[character.OriginCharacter.id];
    }

    public CharacterStatistics GetPastStatistics(RuntimeCharacter character)
    {
        if (pastStatistics.TryGetValue(character.OriginCharacter.id, out CharacterStatistics stat))
        {
            return stat;
        }

        return null;
    }

    public IReadOnlyDictionary<int, CharacterStatistics> GetAllStatistics()
    {
        return currentStatistics;
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
        bool redWin = ReferenceEquals(result.winner, result.statistics.Red.runtimeCharacter);

        RecordCharacter(result.statistics.Red, redWin);
        RecordCharacter(result.statistics.Blue, !redWin);

        RecordMatchup(
            result.statistics.Red.runtimeCharacter,
            result.statistics.Blue.runtimeCharacter,
            redWin);

        RecordMatchup(
            result.statistics.Blue.runtimeCharacter,
            result.statistics.Red.runtimeCharacter,
            !redWin);
    }

    private void RecordCharacter(CharacterBattleStatistics battleStat, bool isWinner)
    {
        CharacterStatistics totalStat = GetStatistics(battleStat.runtimeCharacter);

        PickCount(totalStat);

        totalStat.TotalDamage += battleStat.damageDealt;
        totalStat.TotalSurvivalTime += battleStat.survivalTime;
        totalStat.MoveDistance += battleStat.moveDistance;
        totalStat.AttackCount += battleStat.attackCount;
        totalStat.SkillCount += battleStat.skillCount;

        if (isWinner)
            totalStat.WinCount++;
        else
            totalStat.LoseCount++;
    }

    private void RecordMatchup(
        RuntimeCharacter self,
        RuntimeCharacter opponent,
        bool isWinner)
    {
        MatchupStatistics matchup =
            currentMatchDatas[(self.OriginCharacter.id,
                               opponent.OriginCharacter.id)];

        matchup.MatchCount++;

        if (isWinner)
            matchup.WinCount++;
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

    private float GetPickRate(CharacterStatistics stat, int battleCount)
    {
        if (battleCount == 0) return 0;

        return (float)stat.MatchCount / battleCount * 100f;
    }

    // ========= InspectorWinrateUI

    public MatchUpData GetMatchupData(RuntimeCharacter baseCharacter, RuntimeCharacter opponentCharacter)
    {
        MatchupStatistics current = currentMatchDatas[(baseCharacter.OriginCharacter.id, opponentCharacter.OriginCharacter.id)];
        MatchupStatistics past = pastMatchDatas[(baseCharacter.OriginCharacter.id, opponentCharacter.OriginCharacter.id)];

        MatchUpData data = new()
        {
            HasPastData = HasPastSeasonData,

            CurrentMatchCount = current.MatchCount,
            CurrentWinRate = current.WinRate
        };

        if (HasPastSeasonData)
        {
            data.PastMatchCount = past.MatchCount;
            data.PastWinRate = past.WinRate;
        }

        return data;
    }

    // ========= InspectorAnalysisRowUI

    public AnalysisData GetAnalysis(RuntimeCharacter character, AnalysisItem item)
    {
        CharacterStatistics current = GetStatistics(character);
        CharacterStatistics past = GetPastStatistics(character);

        AnalysisData data = new()
        {
            CurrentValue = GetValue(current, character, item),
            CurrentRank = GetRank(character, item, false),
            HasPastData = HasPastSeasonData
        };

        if (HasPastSeasonData)
        {
            data.PastValue = GetValue(past, character, item);
            data.PastRank = GetRank(character, item, true);
        }

        return data;
    }

    private float GetValue(CharacterStatistics stat, RuntimeCharacter character, AnalysisItem item)
    {
        return item switch
        {
            AnalysisItem.Winrate => stat.WinRate,

            AnalysisItem.Pickrate => GetPickRate(stat, totalBattles),

            AnalysisItem.AverageDamage => stat.AverageDamage,

            AnalysisItem.AverageLiveTme => stat.AverageSurvivalTime,

            AnalysisItem.AverageMoveDistance => stat.AverageMoveDistance,

            AnalysisItem.AverageAttackCount => stat.AverageAttackCount,

            AnalysisItem.AverageSkillCount => stat.AverageSkillCount,

            AnalysisItem.MatchCount => stat.MatchCount,

            AnalysisItem.AverageDPS => stat.AverageDamage / stat.AverageSurvivalTime,

            _ => 0
        };
    }

    private int GetRank(RuntimeCharacter target, AnalysisItem item, bool past)
    {
        List<(RuntimeCharacter character, float value)> list = new();

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            CharacterStatistics stat = past
                ? GetPastStatistics(character)
                : GetStatistics(character);

            float value = GetValue(stat, character, item);

            list.Add((character, value));
        }

        list.Sort((a, b) => b.value.CompareTo(a.value));

        for (int i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i].character, target))
                return i + 1;
        }

        return list.Count;
    }


    public void ResetSeason()
    {
        totalBattles = 0;
        foreach (CharacterStatistics stat in currentStatistics.Values)
        {
            stat.Reset();
        }
    }

    public void MakePast()
    {
        HasPastSeasonData = true;

        pastStatistics.Clear();
        pastTotalBattles = totalBattles;

        foreach (var pair in currentStatistics)
        {
            pastStatistics.Add(pair.Key, new CharacterStatistics(pair.Value));
        }

        pastMatchDatas.Clear();

        foreach (var pair in currentMatchDatas)
        {
            pastMatchDatas.Add(pair.Key, new MatchupStatistics(pair.Value));
        }
    }

}

public class BattleStatistics
{
    public float battleDuration;

    public CharacterBattleStatistics Red { get; private set; }
    public CharacterBattleStatistics Blue { get; private set; }

    public void RegisterRed(CharacterBattleStatistics statistics)
    {
        Red = statistics;
    }

    public void RegisterBlue(CharacterBattleStatistics statistics)
    {
        Blue = statistics;
    }
}

public class AnalysisData
{
    public bool HasPastData;

    public float CurrentValue;
    public float PastValue;

    public int CurrentRank;
    public int PastRank;

}

[Serializable]
public class MatchupStatistics
{
    public int MatchCount;
    public int WinCount;

    public float WinRate => MatchCount == 0 ? 0 : (float)WinCount / MatchCount * 100f;

    public MatchupStatistics() { }

    public MatchupStatistics(MatchupStatistics other)
    {
        MatchCount = other.MatchCount;
        WinCount = other.WinCount;
    }
}
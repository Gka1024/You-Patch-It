using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance { get; private set; }

    private Dictionary<int, CharacterStatistics> currentStatistics = new();
    private Dictionary<int, CharacterStatistics> pastStatistics = new();

    private Dictionary<(int, int), MatchupStatistics> currentMatchDatas = new();
    private Dictionary<(int, int), MatchupStatistics> pastMatchDatas = new();

    public bool HasPastSeasonData { get; private set; }

    public int TotalBattles { get; private set; }
    public int PastTotalBattles { get; private set; }

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

        foreach (Character self in characters)
        {
            foreach (Character enemy in characters)
            {
                currentMatchDatas.Add((self.id, enemy.id), new MatchupStatistics());
                pastMatchDatas.Add((self.id, enemy.id), new MatchupStatistics());
            }
        }

        HasPastSeasonData = false;
        TotalBattles = 0;
        PastTotalBattles = 0;
    }

    // ===== Raw Data =====

    public Dictionary<int, CharacterStatistics> GetAllStatistics()
        => currentStatistics;

    public CharacterStatistics GetCurrentStatistics(int characterId)
        => currentStatistics[characterId];

    public CharacterStatistics GetCurrentStatistics(RuntimeCharacter character)
        => currentStatistics[character.OriginCharacter.id];

    public CharacterStatistics GetPastStatistics(int characterId)
        => pastStatistics.TryGetValue(characterId, out var stat) ? stat : null;

    public CharacterStatistics GetPastStatistics(RuntimeCharacter character)
        => GetPastStatistics(character.OriginCharacter.id);

    public MatchupStatistics GetCurrentMatchup(int selfId, int enemyId)
        => currentMatchDatas[(selfId, enemyId)];

    public MatchupStatistics GetPastMatchup(int selfId, int enemyId)
        => pastMatchDatas[(selfId, enemyId)];

    public IReadOnlyDictionary<int, CharacterStatistics> CurrentStatistics
        => currentStatistics;

    public IReadOnlyDictionary<int, CharacterStatistics> PastStatistics
        => pastStatistics;

    // ===== Record =====

    public void RecordBattle(List<BattleResult> results)
    {
        foreach (BattleResult result in results)
            RecordBattle(result);
    }

    public void RecordBattle(BattleResult result)
    {
        bool redWin =
            ReferenceEquals(result.winner,
                result.statistics.Red.runtimeCharacter);

        RecordCharacter(result.statistics.Red, redWin);
        RecordCharacter(result.statistics.Blue, !redWin);

        RecordMatchup(result.statistics.Red.runtimeCharacter, result.statistics.Blue.runtimeCharacter, redWin);

        RecordMatchup(result.statistics.Blue.runtimeCharacter, result.statistics.Red.runtimeCharacter, !redWin);
    }

    private void RecordCharacter(CharacterBattleStatistics battleStat, bool isWinner)
    {
        CharacterStatistics totalStat = GetCurrentStatistics(battleStat.runtimeCharacter);

        totalStat.MatchCount++;
        TotalBattles++;

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

    private void RecordMatchup(RuntimeCharacter self, RuntimeCharacter enemy, bool isWinner)
    {
        int selfId = self.OriginCharacter.id;
        int enemyId = enemy.OriginCharacter.id;

        if (selfId == enemyId)
            return;

        MatchupStatistics matchup = currentMatchDatas[(selfId, enemyId)];

        matchup.MatchCount++;

        if (isWinner)
            matchup.WinCount++;
    }

    // ===== Season =====

    public void ResetSeason(bool past)
    {
        if(past) MakePast();

        TotalBattles = 0;

        foreach (CharacterStatistics stat in currentStatistics.Values)
            stat.Reset();

        foreach (MatchupStatistics matchup in currentMatchDatas.Values)
        {
            matchup.MatchCount = 0;
            matchup.WinCount = 0;
        }
    }

    public void MakePast()
    {
        HasPastSeasonData = true;

        pastStatistics.Clear();
        PastTotalBattles = TotalBattles;

        foreach (var pair in currentStatistics)
            pastStatistics.Add(
                pair.Key,
                new CharacterStatistics(pair.Value));

        pastMatchDatas.Clear();

        foreach (var pair in currentMatchDatas)
            pastMatchDatas.Add(
                pair.Key,
                new MatchupStatistics(pair.Value));
    }
}

[Serializable]
public class MatchupStatistics
{
    public int MatchCount;
    public int WinCount;

    public float WinRate =>
        MatchCount == 0
            ? 0f
            : (float)WinCount / MatchCount * 100f;

    public MatchupStatistics() { }

    public MatchupStatistics(MatchupStatistics other)
    {
        MatchCount = other.MatchCount;
        WinCount = other.WinCount;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance { get; private set; }

    private Dictionary<int, CharacterStatistics> currentStatistics = new(); // id
    private Dictionary<int, CharacterStatistics> pastStatistics = new();

    private Dictionary<(int, int), MatchupStatistics> currentMatchDatas = new(); // id, id
    private Dictionary<(int, int), MatchupStatistics> pastMatchDatas = new();

    private Dictionary<int, Dictionary<int, Dictionary<int, CharacterStatistics>>> seasonStatistics = new(); // id, Season, Subseason

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
            seasonStatistics.Add(character.id, new Dictionary<int, Dictionary<int, CharacterStatistics>>());
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

    public TierStatistics GetCurrentTierStatistics(RuntimeCharacter character, PlayerTier tier)
    {
        return currentStatistics[character.OriginCharacter.id].TierStatistics[tier];
    }

    public TierStatistics GetPastTierStatistics(RuntimeCharacter character, PlayerTier tier)
    {
        return pastStatistics[character.OriginCharacter.id].TierStatistics[tier];
    }

    public List<CharacterStatistics> GetSeasonStatistics(int characterid, int season)
    {
        if (seasonStatistics.TryGetValue(characterid, out var seasonData))
        {
            if (seasonData.TryGetValue(season, out var data))
            {
                return data.Values.ToList();
            }
        }

        return new List<CharacterStatistics>();
    }

    // ===== Record =====

    public void RecordBattle(List<BattleResult> results)
    {
        foreach (BattleResult result in results)
            RecordBattle(result);
    }

    public void RecordBattle(BattleResult result)
    {
        TotalBattles++;

        for (int i = 0; i < result.statistics.Red.Count; i++)
            RecordCharacter(result.statistics.Red[i], result.redPlayer[i].Tier);

        for (int i = 0; i < result.statistics.Blue.Count; i++)
            RecordCharacter(result.statistics.Blue[i], result.bluePlayer[i].Tier);

        if (result.isDraw)
            return;

        foreach (RuntimeCharacter character in result.winner)
            RecordWinLose(character, GetPlayerTier(result, character), true);

        foreach (RuntimeCharacter character in result.loser)
            RecordWinLose(character, GetPlayerTier(result, character), false);

        RecordMatchups(result);
    }

    private PlayerTier GetPlayerTier(BattleResult result, RuntimeCharacter character)
    {
        for (int i = 0; i < result.statistics.Red.Count; i++)
        {
            if (ReferenceEquals(result.statistics.Red[i].runtimeCharacter, character))
                return result.redPlayer[i].Tier;
        }

        for (int i = 0; i < result.statistics.Blue.Count; i++)
        {
            if (ReferenceEquals(result.statistics.Blue[i].runtimeCharacter, character))
                return result.bluePlayer[i].Tier;
        }

        return PlayerTier.Bronze;
    }

    private void RecordCharacter(CharacterBattleStatistics battleStat, PlayerTier tier)
    {
        CharacterStatistics totalStat = GetCurrentStatistics(battleStat.runtimeCharacter);

        totalStat.MatchCount++;

        totalStat.TotalDamage += battleStat.damageDealt;
        totalStat.TotalSurvivalTime += battleStat.survivalTime;
        totalStat.MoveDistance += battleStat.moveDistance;
        totalStat.AttackCount += battleStat.attackCount;
        totalStat.SkillCount += battleStat.skillCount;

        TierStatistics tierStat = totalStat.TierStatistics[tier];

        tierStat.MatchCount++;
        tierStat.TotalDamage += battleStat.damageDealt;
        tierStat.TotalSurvivalTime += battleStat.survivalTime;
        tierStat.MoveDistance += battleStat.moveDistance;
        tierStat.AttackCount += battleStat.attackCount;
        tierStat.SkillCount += battleStat.skillCount;
    }

    private void RecordWinLose(RuntimeCharacter character, PlayerTier tier, bool isWinner)
    {
        CharacterStatistics stat = GetCurrentStatistics(character);

        if (isWinner) stat.WinCount++;
        else stat.LoseCount++;

        TierStatistics tierStat = stat.TierStatistics[tier];

        if (isWinner) tierStat.WinCount++;
        else tierStat.LoseCount++;
    }

    private void RecordMatchups(BattleResult result)
    {
        foreach (CharacterBattleStatistics red in result.statistics.Red)
        {
            foreach (CharacterBattleStatistics blue in result.statistics.Blue)
            {
                if (red.runtimeCharacter.OriginCharacter.id == blue.runtimeCharacter.OriginCharacter.id)
                    continue;

                bool redWin = result.winner.Contains(red.runtimeCharacter);
                bool blueWin = result.winner.Contains(blue.runtimeCharacter);

                RecordMatchup(red.runtimeCharacter, blue.runtimeCharacter, GetPlayerTier(result, red.runtimeCharacter), redWin);
                RecordMatchup(blue.runtimeCharacter, red.runtimeCharacter, GetPlayerTier(result, blue.runtimeCharacter), blueWin);
            }
        }
    }

    private void RecordMatchup(RuntimeCharacter self, RuntimeCharacter enemy, PlayerTier tier, bool isWinner)
    {
        int selfId = self.OriginCharacter.id;
        int enemyId = enemy.OriginCharacter.id;

        if (selfId == enemyId)
            return;

        MatchupStatistics matchup = currentMatchDatas[(selfId, enemyId)];
        TierMatchupStatistics tierStat = matchup.TierStatistics[tier];

        matchup.MatchCount++;
        tierStat.MatchCount++;

        if (isWinner)
        {
            matchup.WinCount++;
            tierStat.WinCount++;
        }
    }

    // ===== Season =====

    public void SaveCurrentSubSeason(int season, int subSeason)
    {
        foreach (var pair in currentStatistics)
        {
            int id = pair.Key;
            CharacterStatistics stat = pair.Value;

            if (!seasonStatistics.TryGetValue(id, out var seasonData))
            {
                seasonData = new Dictionary<int, Dictionary<int, CharacterStatistics>>();
                seasonStatistics.Add(id, seasonData);
            }

            if (!seasonData.TryGetValue(season, out var subSeasonData))
            {
                subSeasonData = new Dictionary<int, CharacterStatistics>();
                seasonData.Add(season, subSeasonData);
            }

            subSeasonData[subSeason] = new CharacterStatistics(stat);
        }
    }

    public void ResetSeason()
    {
        if (!HasPastSeasonData)
        {
            HasPastSeasonData = true;
        }
        else
        {
            MakePast();
        }

        TotalBattles = 0;

        foreach (CharacterStatistics stat in currentStatistics.Values)
            stat.Reset();

        foreach (MatchupStatistics matchup in currentMatchDatas.Values)
        {
            matchup.MatchCount = 0;
            matchup.WinCount = 0;

            foreach (TierMatchupStatistics tier in matchup.TierStatistics.Values)
            {
                tier.Reset();
            }
        }
    }

    public void MakePast()
    {
        Debug.Log("MakePast");

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

    public Dictionary<PlayerTier, TierMatchupStatistics> TierStatistics = new();

    public float WinRate => MatchCount == 0 ?
    0f : (float)WinCount / MatchCount * 100f;

    public MatchupStatistics()
    {
        foreach (PlayerTier tier in Enum.GetValues(typeof(PlayerTier)))
        {
            TierStatistics.Add(tier, new TierMatchupStatistics());
        }
    }
    public MatchupStatistics(MatchupStatistics other)
    {
        MatchCount = other.MatchCount;
        WinCount = other.WinCount;

        TierStatistics = new();

        foreach (var pair in other.TierStatistics)
        {
            TierStatistics.Add(
                pair.Key,
                new TierMatchupStatistics(pair.Value));
        }
    }
}

public class TierMatchupStatistics
{
    public int MatchCount;
    public int WinCount;

    public float WinRate =>
        MatchCount == 0 ? 0f : (float)WinCount / MatchCount * 100f;

    public TierMatchupStatistics() { }

    public TierMatchupStatistics(TierMatchupStatistics other)
    {
        MatchCount = other.MatchCount;
        WinCount = other.WinCount;
    }

    public void Reset()
    {
        MatchCount = 0;
        WinCount = 0;
    }
}
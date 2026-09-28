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

    private Dictionary<int, Dictionary<int, Dictionary<int, CharacterStatistics>>> seasonStatistics = new();
    private Dictionary<int, Dictionary<int, int>> seasonBattleCounts = new();
    private Dictionary<int, Dictionary<int, Dictionary<int, int>>> seasonPickCounts = new();

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

        seasonStatistics.Clear();
        seasonBattleCounts.Clear();
        seasonPickCounts.Clear();

        List<Character> characters = database.GetAllCharacters().ToList();

        foreach (Character character in characters)
        {
            currentStatistics.Add(character.id, new CharacterStatistics());
            pastStatistics.Add(character.id, new CharacterStatistics());

            seasonStatistics.Add(character.id, new Dictionary<int, Dictionary<int, CharacterStatistics>>());
            seasonPickCounts.Add(character.id, new Dictionary<int, Dictionary<int, int>>());
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

    // =========================================================
    // Raw Data
    // =========================================================

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

    public List<CharacterStatistics> GetSeasonStatistics(int characterId, int season)
    {
        if (seasonStatistics.TryGetValue(characterId, out var seasonData))
        {
            if (seasonData.TryGetValue(season, out var subSeasonData))
            {
                return subSeasonData
                    .OrderBy(pair => pair.Key)
                    .Select(pair => pair.Value)
                    .ToList();
            }
        }

        return new List<CharacterStatistics>();
    }

    // =========================================================
    // Season Pick Rate
    // =========================================================

    public float GetSeasonAveragePickRate(int characterId, int season, int teamSize)
    {
        if (teamSize <= 0)
            return 0f;

        if (!seasonPickCounts.TryGetValue(characterId, out var characterData))
            return 0f;

        if (!characterData.TryGetValue(season, out var pickData))
            return 0f;

        if (!seasonBattleCounts.TryGetValue(season, out var battleData))
            return 0f;

        float totalPickRate = 0f;
        int validSubSeasonCount = 0;

        foreach (var pair in pickData.OrderBy(pair => pair.Key))
        {
            int subSeason = pair.Key;
            int pickCount = pair.Value;

            if (!battleData.TryGetValue(subSeason, out int cumulativeBattleCount))
                continue;

            int previousBattleCount = battleData
                .Where(b => b.Key < subSeason)
                .Select(b => b.Value)
                .DefaultIfEmpty(0)
                .Max();

            int subSeasonBattleCount = cumulativeBattleCount - previousBattleCount;

            if (subSeasonBattleCount <= 0)
                continue;

            int totalCharacterSlots = subSeasonBattleCount * teamSize * 2;

            float pickRate = (float)pickCount / totalCharacterSlots * 100f;

            totalPickRate += pickRate;
            validSubSeasonCount++;
        }

        if (validSubSeasonCount == 0)
            return 0f;

        return totalPickRate / validSubSeasonCount;
    }

    // =========================================================
    // Record
    // =========================================================

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

        if (isWinner)
            stat.WinCount++;
        else
            stat.LoseCount++;

        TierStatistics tierStat = stat.TierStatistics[tier];

        if (isWinner)
            tierStat.WinCount++;
        else
            tierStat.LoseCount++;
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

    // =========================================================
    // Season
    // =========================================================

    public void SaveCurrentSubSeason(int season, int subSeason)
    {
        // 이번 시즌의 서브시즌별 누적 전투 횟수 저장
        if (!seasonBattleCounts.TryGetValue(season, out var battleData))
        {
            battleData = new Dictionary<int, int>();
            seasonBattleCounts.Add(season, battleData);
        }

        battleData[subSeason] = TotalBattles;

        foreach (var pair in currentStatistics)
        {
            int id = pair.Key;
            CharacterStatistics stat = pair.Value;

            // 시즌 통계 저장소 확보
            if (!seasonStatistics[id].TryGetValue(season, out var subSeasonData))
            {
                subSeasonData = new Dictionary<int, CharacterStatistics>();
                seasonStatistics[id].Add(season, subSeasonData);
            }

            // 현재 서브시즌 이전의 가장 최근 스냅샷 검색
            int previousPickCount = 0;

            var previousEntry = subSeasonData
                .Where(data => data.Key < subSeason)
                .OrderByDescending(data => data.Key)
                .FirstOrDefault();

            if (previousEntry.Value != null)
            {
                previousPickCount = previousEntry.Value.MatchCount;
            }

            // 누적 픽 횟수에서 이전 누적 픽 횟수를 빼 실제 픽 횟수 계산
            int subSeasonPickCount = stat.MatchCount - previousPickCount;

            // 현재 누적 통계 스냅샷 저장
            subSeasonData[subSeason] = new CharacterStatistics(stat);

            // 실제 서브시즌 픽 횟수 저장
            if (!seasonPickCounts[id].TryGetValue(season, out var pickData))
            {
                pickData = new Dictionary<int, int>();
                seasonPickCounts[id].Add(season, pickData);
            }

            pickData[subSeason] = subSeasonPickCount;
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
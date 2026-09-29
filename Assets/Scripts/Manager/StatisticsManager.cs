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
    private Dictionary<int, Dictionary<int, int>> seasonBattleCounts = new(); // 각각 순서대로 메인시즌, 서브시즌, 전투 횟수
    private Dictionary<int, Dictionary<int, Dictionary<int, int>>> seasonPickCounts = new(); // 각각 순서대로 캐릭터 ID, 메인 시즌, 서브 시즌, 픽 횟수

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

    public float GetSeasonPickRate(int characterId, int season, int teamSize)
    {
        if (teamSize <= 0)
            return 0f;

        if (!seasonPickCounts.TryGetValue(characterId, out var characterData))
            return 0f;

        if (!characterData.TryGetValue(season, out var pickData))
            return 0f;

        if (!seasonBattleCounts.TryGetValue(season, out var battleData))
            return 0f;

        int totalPickCount = 0;

        for (int subSeason = 1; subSeason <= 3; subSeason++)
        {
            if (pickData.TryGetValue(subSeason, out int pickCount))
            {
                totalPickCount += pickCount;
            }
        }

        int totalBattleCount = battleData.Values.DefaultIfEmpty(0).Max();

        if (totalBattleCount <= 0)
            return 0f;

        int totalCharacterSlots = totalBattleCount * teamSize * 2;

        Debug.Log($"Season: {season}");
        Debug.Log($"SubSeason 1: {pickData.GetValueOrDefault(1)}");
        Debug.Log($"SubSeason 2: {pickData.GetValueOrDefault(2)}");
        Debug.Log($"SubSeason 3: {pickData.GetValueOrDefault(3)}");

        return (float)totalPickCount / totalCharacterSlots * 100f;
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
        // 1. 메인 시즌의 서브시즌별 전투 횟수 저장
        if (!seasonBattleCounts.TryGetValue(season, out var battleData))
        {
            battleData = new Dictionary<int, int>();
            seasonBattleCounts.Add(season, battleData);
        }

        // ResetSeason()이 서브시즌마다 호출되므로 현재 전투 횟수를 그대로 저장
        battleData[subSeason] = TotalBattles;

        // 2. 캐릭터별 현재 서브시즌 통계 저장
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

            // 현재 서브시즌의 통계 스냅샷 저장
            subSeasonData[subSeason] = new CharacterStatistics(stat);

            // 3. 캐릭터별 서브시즌 픽 횟수 저장
            if (!seasonPickCounts[id].TryGetValue(season, out var pickData))
            {
                pickData = new Dictionary<int, int>();
                seasonPickCounts[id].Add(season, pickData);
            }

            // 이전 서브시즌의 누적값을 빼지 않고 현재 MatchCount를 그대로 저장
            pickData[subSeason] = stat.MatchCount;
        }

        Debug.Log($"[SaveCurrentSubSeason] Season: {season}, SubSeason: {subSeason}, Battles: {TotalBattles}");
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
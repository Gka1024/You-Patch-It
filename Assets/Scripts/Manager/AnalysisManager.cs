using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnalysisManager : MonoBehaviour
{
    public static AnalysisManager Instance { get; private set; }

    private readonly Dictionary<bool, Dictionary<AnalysisItem, Dictionary<int, int>>> rankCache = new();
    private Dictionary<(bool past, int characterid, AnalysisItem item), float> valueCache = new();
    private void Awake()
    {
        Instance = this;
    }

    // 시즌 결과 기록 후 호출
    public void AnalyzeSeason()
    {
        valueCache.Clear();

        BuildRankCache(false);

        if (StatisticsManager.Instance.HasPastSeasonData)
        {
            BuildRankCache(true);
        }
    }

    // ===== Public API =====

    public float GetPickRate(RuntimeCharacter character)
    {
        StatisticsManager statisticsManager = StatisticsManager.Instance;

        if (statisticsManager.TotalBattles == 0)
            return 0f;

        CharacterStatistics stat = statisticsManager.GetCurrentStatistics(character);

        return (float)stat.MatchCount / statisticsManager.TotalBattles * 100f;
    }

    public MatchUpData GetMatchupData(RuntimeCharacter self, RuntimeCharacter enemy)
    {
        StatisticsManager statisticsManager = StatisticsManager.Instance;

        MatchupStatistics current = statisticsManager.GetCurrentMatchup(self.OriginCharacter.id, enemy.OriginCharacter.id);

        MatchUpData data = new()
        {
            HasPastData = statisticsManager.HasPastSeasonData,
            CurrentMatchCount = current.MatchCount,
            CurrentWinRate = current.WinRate
        };

        if (statisticsManager.HasPastSeasonData)
        {
            MatchupStatistics past = statisticsManager.GetPastMatchup(self.OriginCharacter.id, enemy.OriginCharacter.id);

            data.PastMatchCount = past.MatchCount;
            data.PastWinRate = past.WinRate;
        }

        return data;
    }

    public AnalysisData GetAnalysis(RuntimeCharacter character, AnalysisItem item)
    {
        StatisticsManager statisticsManager = StatisticsManager.Instance;

        AnalysisData data = new()
        {
            HasPastData = statisticsManager.HasPastSeasonData,
            CurrentValue = GetValue(character, item, false),
            CurrentRank = GetRank(character, item, false)
        };

        if (statisticsManager.HasPastSeasonData)
        {
            data.PastValue = GetValue(character, item, true);
            data.PastRank = GetRank(character, item, true);
        }

        return data;
    }

    // ===== Value =====

    private float GetValue(RuntimeCharacter character, AnalysisItem item, bool past)
    {
        var key = (past, character.OriginCharacter.id, item);

        if (valueCache.TryGetValue(key, out float value))
            return value;

        StatisticsManager statisticsManager = StatisticsManager.Instance;

        CharacterStatistics stat = past
            ? statisticsManager.GetPastStatistics(character)
            : statisticsManager.GetCurrentStatistics(character);

        if (stat == null)
            return 0f;

        int battleCount = past
            ? statisticsManager.PastTotalBattles
            : statisticsManager.TotalBattles;

        value = item switch
        {
            AnalysisItem.Winrate => stat.WinRate,
            AnalysisItem.Pickrate => battleCount == 0 ? 0f : (float)stat.MatchCount / battleCount * 100f,
            AnalysisItem.AverageDamage => stat.AverageDamage,
            AnalysisItem.AverageLiveTime => stat.AverageSurvivalTime,
            AnalysisItem.AverageMoveDistance => stat.AverageMoveDistance,
            AnalysisItem.AverageAttackCount => stat.AverageAttackCount,
            AnalysisItem.AverageSkillCount => stat.AverageSkillCount,
            AnalysisItem.MatchCount => stat.MatchCount,
            AnalysisItem.AverageDPS => stat.AverageSurvivalTime <= 0f ? 0f : stat.AverageDamage / stat.AverageSurvivalTime,
            _ => 0f
        };

        valueCache[key] = value;

        return value;
    }

    // ===== Rank Cache =====

    private void BuildRankCache(bool past)
    {
        Dictionary<AnalysisItem, Dictionary<int, int>> itemRanks = new();

        foreach (AnalysisItem item in Enum.GetValues(typeof(AnalysisItem)))
        {
            List<(int id, float value)> list = new();

            foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
            {
                int id = character.OriginCharacter.id;

                list.Add((id, GetValue(character, item, past)));
            }

            list.Sort((a, b) => b.value.CompareTo(a.value));

            Dictionary<int, int> ranks = new();

            for (int i = 0; i < list.Count; i++)
                ranks[list[i].id] = i + 1;

            itemRanks[item] = ranks;
        }

        rankCache[past] = itemRanks;
    }

    private int GetRank(RuntimeCharacter character, AnalysisItem item, bool past)
    {
        int id = character.OriginCharacter.id;

        if (!rankCache.TryGetValue(past, out var itemRanks))
            return 0;

        if (!itemRanks.TryGetValue(item, out var ranks))
            return 0;

        return ranks.TryGetValue(id, out int rank)
            ? rank
            : 0;
    }

    public List<RuntimeCharacter> GetSortedCharacters(AnalysisItem item, SortDirection direction, bool past = false)
    {
        List<RuntimeCharacter> list = RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        list.Sort((a, b) =>
        {
            float av = GetValue(a, item, past);
            float bv = GetValue(b, item, past);

            int compare = av.CompareTo(bv);

            if (compare == 0)
            {
                compare = a.OriginCharacter.id.CompareTo(b.OriginCharacter.id);
            }

            return direction == SortDirection.Descending
                ? -compare
                : compare;
        });

        return list;
    }
}

[Serializable]
public class AnalysisData
{
    public bool HasPastData;

    public float CurrentValue;
    public float PastValue;

    public int CurrentRank;
    public int PastRank;
}
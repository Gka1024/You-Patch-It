using System;
using System.Collections.Generic;

[Serializable]
public class CharacterStatistics
{
    public int MatchCount;

    public int WinCount;

    public int LoseCount;

    public float TotalDamage;

    public int AttackCount;

    public int SkillCount;

    public float MoveDistance;

    public float TotalSurvivalTime;

    public float WinRate => MatchCount == 0 ? 0 : (float)WinCount / (WinCount + LoseCount) * 100f;

    public float AverageDamage => MatchCount == 0 ? 0 : TotalDamage / MatchCount;

    public float AverageSurvivalTime => MatchCount == 0 ? 0 : TotalSurvivalTime / MatchCount;

    public float AverageAttackCount => MatchCount == 0 ? 0 : AttackCount / MatchCount;

    public float AverageMoveDistance => MatchCount == 0 ? 0 : MoveDistance / MatchCount;

    public float AverageSkillCount => MatchCount == 0 ? 0 : SkillCount / MatchCount;

    public Dictionary<PlayerTier, TierStatistics> TierStatistics = new();

    public void Reset()
    {
        MatchCount = 0;
        WinCount = 0;
        LoseCount = 0;
        TotalDamage = 0;
        TotalSurvivalTime = 0;
        AttackCount = 0;
        SkillCount = 0;
        MoveDistance = 0;

        foreach (TierStatistics stat in TierStatistics.Values)
        {
            stat.Reset();
        }
    }

    public CharacterStatistics()
    {
        foreach (PlayerTier tier in Enum.GetValues(typeof(PlayerTier)))
        {
            TierStatistics.Add(tier, new TierStatistics());
        }
    }

    public CharacterStatistics(CharacterStatistics other)
    {
        MatchCount = other.MatchCount;
        WinCount = other.WinCount;
        LoseCount = other.LoseCount;
        TotalDamage = other.TotalDamage;
        TotalSurvivalTime = other.TotalSurvivalTime;
        AttackCount = other.AttackCount;
        SkillCount = other.SkillCount;
        MoveDistance = other.MoveDistance;

        TierStatistics = new();

        foreach (var pair in other.TierStatistics)
        {
            TierStatistics.Add(pair.Key, new TierStatistics(pair.Value));
        }
    }
}
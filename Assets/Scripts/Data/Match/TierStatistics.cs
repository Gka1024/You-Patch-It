using System;
using System.Collections.Generic;

[Serializable]
public class TierStatistics
{
    public int MatchCount;
    public int WinCount;

    public float TotalDamage;
    public float TotalSurvivalTime;
    public float MoveDistance;
    public int AttackCount;
    public int SkillCount;

    public float WinRate =>
        MatchCount == 0
            ? 0
            : (float)WinCount / MatchCount * 100f;

    public float AverageDamage =>
        MatchCount == 0 ? 0 : TotalDamage / MatchCount;

    public float AverageSurvivalTime =>
        MatchCount == 0 ? 0 : TotalSurvivalTime / MatchCount;

    public float AverageMoveDistance =>
        MatchCount == 0 ? 0 : MoveDistance / MatchCount;

    public float AverageAttackCount =>
        MatchCount == 0 ? 0 : (float)AttackCount / MatchCount;

    public float AverageSkillCount =>
        MatchCount == 0 ? 0 : (float)SkillCount / MatchCount;

    public void Reset()
    {
        MatchCount = 0;
        WinCount = 0;

        TotalDamage = 0;
        TotalSurvivalTime = 0;
        MoveDistance = 0;
        AttackCount = 0;
        SkillCount = 0;
    }

    public TierStatistics()
    {

    }

    public TierStatistics(TierStatistics other)
    {
        MatchCount = other.MatchCount;
        WinCount = other.WinCount;

        TotalDamage = other.TotalDamage;
        TotalSurvivalTime = other.TotalSurvivalTime;
        MoveDistance = other.MoveDistance;
        AttackCount = other.AttackCount;
        SkillCount = other.SkillCount;
    }
}
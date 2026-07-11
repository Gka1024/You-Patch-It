using System;

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

    public float WinRate => MatchCount == 0 ? 0 : (float)WinCount / MatchCount * 100f;

    public float AverageDamage => MatchCount == 0 ? 0 : TotalDamage / MatchCount;

    public float AverageSurvivalTime => MatchCount == 0 ? 0 : TotalSurvivalTime / MatchCount;

    public float AverageAttackCount => MatchCount == 0 ? 0 : AttackCount / MatchCount;

    public float AverageMoveDistance => MatchCount == 0 ? 0 : MoveDistance / MatchCount;

    public float AverageSkillCount => MatchCount == 0 ? 0 : SkillCount / MatchCount;


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
    }

    public CharacterStatistics() { }

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
    }
}
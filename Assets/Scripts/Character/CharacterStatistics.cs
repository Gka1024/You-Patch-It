using System;

[Serializable]
public class CharacterStatistics
{
    public int MatchCount;

    public int WinCount;

    public int LoseCount;

    public int PickCount;

    public float TotalDamage;

    public float TotalSurvivalTime;

    public float WinRate =>
        MatchCount == 0 ? 0 : (float)WinCount / MatchCount * 100f;

    public float PickRate;

    public float AverageDamage =>
        MatchCount == 0 ? 0 : TotalDamage / MatchCount;

    public float AverageSurvivalTime =>
        MatchCount == 0 ? 0 : TotalSurvivalTime / MatchCount;

    public void Reset()
    {
        MatchCount = 0;
        WinCount = 0;
        LoseCount = 0;
        PickCount = 0;
        TotalDamage = 0;
        TotalSurvivalTime = 0;
        PickRate = 0;
    }
}
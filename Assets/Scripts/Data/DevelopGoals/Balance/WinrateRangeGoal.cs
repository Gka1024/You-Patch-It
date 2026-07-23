public class WinrateRangeGoal : DeveloperGoal
{
    private readonly float minWinrate;
    private readonly float maxWinrate;

    public override string Title => "선 넘네";
    public override string Description =>
    $"모든 캐릭터의 승률을 {minWinrate}% ~ {maxWinrate}% 사이로 유지하세요.";

    public WinrateRangeGoal(float minWinrate, float maxWinrate, GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        this.minWinrate = minWinrate;
        this.maxWinrate = maxWinrate;
    }

    protected override bool CheckCompleted()
    {
        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            CharacterStatistics stat = StatisticsManager.Instance.GetCurrentStatistics(character);

            if (stat.MatchCount == 0) continue;

            if (stat.WinRate < minWinrate) return false;
            if (stat.WinRate > maxWinrate) return false;
        }

        return true;
    }

    public override float GetCurrentProgress()
    {
        return CheckCompleted() ? 1 : 0;
    }
}
using UnityEngine;

public class WinrateBandGoal : DeveloperGoal
{
    private readonly float minWinrate;
    private readonly float maxWinrate;
    private readonly int requiredCount;

    public override string Title => "인플레이션";

    public override string Description =>
        $"승률이 {minWinrate}%~{maxWinrate}%인 캐릭터를 {requiredCount}명 이상 만드세요.";

    public WinrateBandGoal(float min, float max, int count, GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        minWinrate = min;
        maxWinrate = max;
        requiredCount = count;
    }

    protected override bool CheckCompleted()
    {
        return CountQualifiedCharacters() >= requiredCount;
    }

    public override float GetCurrentProgress()
    {
        return Mathf.Clamp01(
            (float)CountQualifiedCharacters() / requiredCount);
    }

    private int CountQualifiedCharacters()
    {
        int count = 0;

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            float winrate =
                StatisticsManager.Instance
                    .GetCurrentStatistics(character)
                    .WinRate;

            if (winrate >= minWinrate &&
                winrate <= maxWinrate)
            {
                count++;
            }
        }

        return count;
    }
}
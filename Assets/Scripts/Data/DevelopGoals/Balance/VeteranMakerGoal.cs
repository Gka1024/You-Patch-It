using UnityEngine;

public class VeteranMakerGoal : DeveloperGoal
{
    public readonly float winrate;

    public override string Title => "고인물 제조기";

    public override string Description =>
        $"승률이 {winrate}% 이상인 캐릭터를 단 하나만 만드세요.";

    public VeteranMakerGoal(float winrate, GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        this.winrate = winrate;
    }

    protected override bool CheckCompleted()
    {
        int count = 0;

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            float winrate =
                StatisticsManager.Instance
                    .GetCurrentStatistics(character)
                    .WinRate;

            if (winrate >= 55f)
                count++;
        }

        return count == 1;
    }

    public override float GetCurrentProgress()
    {
        int count = 0;

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            float winrate =
                StatisticsManager.Instance
                    .GetCurrentStatistics(character)
                    .WinRate;

            if (winrate >= 55f)
                count++;
        }

        if (count == 1)
            return 1f;

        if (count == 0)
            return 0.8f;

        return Mathf.Clamp01(1f / count);
    }
}
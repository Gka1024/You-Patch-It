using System.Collections.Generic;
using System.Linq;

public class ReverseMetaGoal : DeveloperGoal
{
    public override string Title => "확실한 메타인지";

    public override string Description =>
        "지난 시즌 승률 상위 3명과 하위 3명이 이번 시즌에도 같은 위치에 있지 않게 하세요.";

    public ReverseMetaGoal(GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
    }

    protected override bool CheckCompleted()
    {
        return GetCurrentProgress() >= 1f;
    }

    public override float GetCurrentProgress()
    {
        List<RuntimeCharacter> currentRank =
            AnalysisManager.Instance.GetSortedCharacters(
                AnalysisItem.Winrate,
                SortDirection.Descending,
                false);

        List<RuntimeCharacter> previousRank =
            AnalysisManager.Instance.GetSortedCharacters(
                AnalysisItem.Winrate,
                SortDirection.Descending,
                true);

        if (currentRank.Count < 3 || previousRank.Count < 3)
            return 0f;

        int overlap = 0;

        // Top3
        for (int i = 0; i < 3; i++)
        {
            if (previousRank.Take(3).Contains(currentRank[i]))
                overlap++;
        }

        // Bottom3
        for (int i = 0; i < 3; i++)
        {
            if (previousRank.TakeLast(3).Contains(currentRank[currentRank.Count - 1 - i]))
                overlap++;
        }


        return 1f - overlap / 6f;
    }
}
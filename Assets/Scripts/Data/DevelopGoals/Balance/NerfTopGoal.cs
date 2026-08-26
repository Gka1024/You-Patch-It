using System.Linq;
using UnityEngine;

public class NerfTopGoal : DeveloperGoal
{
    private RuntimeCharacter targetCharacter;
    private readonly float winrate;

    public override string Title => "유일신 없애기";

    public override string Description =>
        $"지난 시즌 최상위였던 {targetCharacter.OriginCharacter.characterName}을(를) 이번 시즌 승률을 감소시키세요.";

    public NerfTopGoal(GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        Refresh();
    }

    public override void Refresh()
    {
        targetCharacter = GetTarget();
    }

    public RuntimeCharacter GetTarget()
    {
        RuntimeCharacter returnCharacter = RuntimeCharacterManager.Instance.GetRuntimeCharacter(101);
        float winrate = 0f;

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            if (StatisticsManager.Instance.GetCurrentStatistics(character).Winrate  > winrate)
            {
                returnCharacter = character;
                winrate =StatisticsManager.Instance.GetCurrentStatistics(character).Winrate;
            }
        }

        return returnCharacter;
    }

    protected override bool CheckCompleted()
    {
        return AnalysisManager.Instance.GetAnalysis(targetCharacter, AnalysisItem.Winrate).CurrentValue < winrate;
    }

    public override float GetCurrentProgress()
    {
        return AnalysisManager.Instance.GetAnalysis(targetCharacter, AnalysisItem.Winrate).CurrentValue < winrate ? 100f : 0f;
    }
}
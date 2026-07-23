using System;
using UnityEngine;

public class RolePickrateGoal : DeveloperGoal
{
    private readonly float minPickrate;

    public override string Title => "다양한 메타";
    public override string Description =>
        $"모든 역할의 평균 픽률을 {minPickrate}% 이상으로 유지하세요.";

    public RolePickrateGoal(float minPickrate, GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        this.minPickrate = minPickrate;
    }

    protected override bool CheckCompleted()
    {
        foreach (CharacterRole role in Enum.GetValues(typeof(CharacterRole)))
        {
            float average =
                AnalysisManager.Instance.GetAveragePickRate(role);

            if (average < minPickrate)
                return false;
        }

        return true;
    }

    public override float GetCurrentProgress()
    {
        float total = 0;

        foreach (CharacterRole role in Enum.GetValues(typeof(CharacterRole)))
        {
            float average =
                AnalysisManager.Instance.GetAveragePickRate(role);

            total += Mathf.Clamp01(average / minPickrate);
        }

        return total / Enum.GetValues(typeof(CharacterRole)).Length;
    }
}
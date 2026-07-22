public class MaxPickRateGoal : DeveloperGoal
{
    private readonly float maxPickRate;

    public override string Title => "독과점 금지!";
    public override string Description =>
    $"가장 높은 픽률을 {maxPickRate}% 미만으로 유지하세요.";

    public MaxPickRateGoal(float maxPickRate, GoalDifficulty difficulty) : base(difficulty)
    {
        this.maxPickRate = maxPickRate;
    }

    protected override bool CheckCompleted()
    {
        return AnalysisManager.Instance.GetMaxValue(AnalysisItem.Pickrate, false) < 20f;
    }

    public override float GetCurrentProgress()
    {
        return CheckCompleted() ? 1 : 0;
    }
}
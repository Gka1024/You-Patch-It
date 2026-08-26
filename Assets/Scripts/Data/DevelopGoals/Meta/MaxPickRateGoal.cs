public class MaxPickRateGoal : DeveloperGoal
{
    private readonly float maxPickRate;

    public override string Title => "독과점 금지!";
    public override string Description =>
    $"가장 높은 픽률을 {maxPickRate}% 미만으로 유지하세요.";

    public MaxPickRateGoal(float maxPickRate, GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        this.maxPickRate = maxPickRate;
    }

    public override void Refresh()
    {

    }

    protected override bool CheckCompleted()
    {
        return AnalysisManager.Instance.GetMaxValue(AnalysisItem.Pickrate, false) < maxPickRate;
    }

    public override float GetCurrentProgress()
    {
        return CheckCompleted() ? 1 : 0;
    }
}
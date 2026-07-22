public abstract class DeveloperGoal
{
    public abstract string Title { get; }
    public abstract string Description { get; }

    public GoalReward Reward { get; }

    public bool IsComplete { get; private set; }

    protected DeveloperGoal(GoalDifficulty difficulty)
    {
        Reward = GoalManager.Instance.GetReward(difficulty);
    }

    public void Evaluate()
    {
        IsComplete = CheckCompleted();
    }

    protected abstract bool CheckCompleted();
    public abstract float GetCurrentProgress();
}
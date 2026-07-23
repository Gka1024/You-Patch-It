public abstract class DeveloperGoal
{
    public abstract string Title { get; }
    public abstract string Description { get; }

    public GoalType type;

    public GoalReward Reward { get; }

    public bool IsComplete { get; private set; }

    protected DeveloperGoal(GoalDifficulty difficulty, GoalType goalType)
    {
        Reward = GoalManager.Instance.GetReward(difficulty);
        type = goalType;
    }

    public void Evaluate()
    {
        IsComplete = CheckCompleted();
    }

    protected abstract bool CheckCompleted();
    public abstract float GetCurrentProgress();
}

public enum GoalType
{
    Balance,
    Meta,
    Patch,
    Challenge
}
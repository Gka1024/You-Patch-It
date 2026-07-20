public abstract class DeveloperGoal
{
    public abstract string Title { get; }
    public abstract string Description { get; }

    public int DevelopResourceReward;
    public int TrustPointReward;

    public bool IsComplete { get; private set; }
    
    public void Evaluate()
    {
        IsComplete = CheckCompleted();
    }

    protected abstract bool CheckCompleted();
    
}
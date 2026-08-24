public class UnlockTutorialUI : TutorialIndexUI
{
    public DashBoardUI dashboard;

    private const int GOAL_ADDITIONAL_SLOT_1 = 2021;

    protected void Start()
    {
        dashboard.OnUnlockUIOpen += UnlockUIOpen;
        UnlockManager.Instance.OnUnlockChanged += OnUnlockGoal;
    }

    private void UnlockUIOpen()
    {
        if (index != 1)
            return;

        GoalManager.Instance.OnGoalChanged -= UnlockUIOpen;
        EnterNextPage();

    }

    private void OnUnlockGoal()
    {
        if (index != 4)
            return;

        if (UnlockManager.Instance.IsUnlocked(GOAL_ADDITIONAL_SLOT_1))
        {
            GoalManager.Instance.OnGoalChanged -= OnUnlockGoal;
            EnterNextPage();
        }

    }

}
public class UnlockTutorialUI : TutorialIndexUI
{
    public DashBoardUI dashboard;

    protected void Start()
    {
        dashboard.OnUnlockUIOpen += UnlockUIOpen;
    }

    private void UnlockUIOpen()
    {
        if (index != 1)
            return;

        GoalManager.Instance.OnGoalChanged -= UnlockUIOpen;
        EnterNextPage();

    }

}
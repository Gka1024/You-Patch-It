public class GoalTutorialUI : TutorialIndexUI
{
    protected void Start()
    {
        GoalManager.Instance.OnGoalChanged += OnGoalChanged;
        GoalManager.Instance.OnGoalConfirmed += OnGoalConfirmed;
    }

    void OnDisable()
    {
        GoalManager.Instance.OnGoalChanged -= OnGoalChanged;
        GoalManager.Instance.OnGoalConfirmed -= OnGoalConfirmed;
    }

    private void OnGoalChanged()
    {
        if (index != 3)
            return;

        GoalManager.Instance.OnGoalChanged -= OnGoalChanged; ;
        EnterNextPage();

    }

    private void OnGoalConfirmed()
    {
        if (index != 4)
            return;

        GoalManager.Instance.OnGoalConfirmed -= OnGoalConfirmed;
        EnterNextPage();
    }
}
public class EndTutorialUI : TutorialIndexUI
{
    private const int ADD_GAME_3VS3 = 3051;

    protected void Start()
    {
        SeasonManager.Instance.OnSeasonEnd += OnSeasonEnd;
        UnlockManager.Instance.OnUnlockChanged += OnTutorialEnd;
    }

    void OnDestroy()
    {
        SeasonManager.Instance.OnSeasonEnd -= OnSeasonEnd;
        UnlockManager.Instance.OnUnlockChanged -= OnTutorialEnd;
    }

    private void OnSeasonEnd()
    {
        if (index != 0) return;

        SeasonManager.Instance.OnSeasonEnd -= OnSeasonEnd;
        EnterNextPage();

    }

    private void OnTutorialEnd()
    {
        if (index != 4) return;

        if (UnlockManager.Instance.IsUnlocked(ADD_GAME_3VS3))
        {
            UnlockManager.Instance.OnUnlockChanged -= OnTutorialEnd;

        }
        else
        {
            EnterNextPage();
        }

    }

}
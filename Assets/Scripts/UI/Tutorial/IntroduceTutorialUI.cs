public class IntroduceTutorialUI : TutorialIndexUI
{
    protected void Start()
    {
        PatchManager.Instance.OnPatchConfirmed += OnSeasonProceeded;
    }

    void OnDestroy()
    {
        PatchManager.Instance.OnPatchConfirmed -= OnSeasonProceeded;

    }

    private void OnSeasonProceeded()
    {
        if (index != 3)
            return;

        PatchManager.Instance.OnPatchConfirmed -= OnSeasonProceeded;
        EnterNextPage();

    }
}
public class PatchTutorialUI : TutorialIndexUI
{
    public SeasonReportUI report;
    
    protected void Start()
    {
        PatchManager.Instance.OnPatchApplied += OnCharacterPatched;
        PatchManager.Instance.OnPatchConfirmed += OnPatchConfirmed;
        PatchManager.Instance.OnPatchApplied += OnCharacterPatched2;
        PatchManager.Instance.OnPatchConfirmed += OnPatchConfirmed2;
        report.OnProceed += OnProceed;
    }

    void OnDisable()
    {
        PatchManager.Instance.OnPatchApplied -= OnCharacterPatched;
        PatchManager.Instance.OnPatchConfirmed -= OnPatchConfirmed;
        PatchManager.Instance.OnPatchApplied -= OnCharacterPatched2;
        PatchManager.Instance.OnPatchConfirmed -= OnPatchConfirmed2;
        report.OnProceed += OnProceed;
    }

    private void OnCharacterPatched(PatchRecord record)
    {
        if (index != 2) return;

        PatchManager.Instance.OnPatchApplied -= OnCharacterPatched;
        EnterNextPage();

    }

    private void OnPatchConfirmed()
    {
        if (index != 4) return;

        PatchManager.Instance.OnPatchConfirmed -= OnPatchConfirmed;
        EnterNextPage();
    }

    private void OnCharacterPatched2(PatchRecord record)
    {
        if (index != 7) return;

        PatchManager.Instance.OnPatchApplied -= OnCharacterPatched2;
        EnterNextPage();

    }

    private void OnPatchConfirmed2()
    {
        if (index != 8) return;

        PatchManager.Instance.OnPatchConfirmed -= OnPatchConfirmed2;
        EnterNextPage();
    }

     private void OnProceed()
    {
        if (index != 11) return;

        SeasonReportUI.Instance.OnProceed -= OnProceed;
        EnterNextPage();
    }
}
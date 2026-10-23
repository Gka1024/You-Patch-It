using UnityEngine;
using UnityEngine.UI;

public class DashBoardUI : MonoBehaviour
{
    [SerializeField] private Button CharacterButton;
    [SerializeField] private Button PatchNoteButton;
    [SerializeField] private Button GraphButton;
    [SerializeField] private Button SeasonReportButton;
    [SerializeField] private Button UnlockButton;
    [SerializeField] private Button GoalButton;
    [SerializeField] private Button ReportButton;

    [Header("Character Table")]
    [SerializeField] private GameObject CharacterTable;
    [SerializeField] private GameObject Inspector;
    [SerializeField] private GameObject BottomDisplay;
    [SerializeField] private GameObject CharacterPreview;

    [Header("Patch Notes")]
    [SerializeField] private GameObject PatchNotes;

    [Header("Graphs")]
    [SerializeField] private GameObject Graphs;

    [Header("Unlock System")]
    [SerializeField] private GameObject UnlockUI;
    public event System.Action OnUnlockUIOpen;

    [Header("Developer Goals")]
    [SerializeField] private GameObject DeveloperGoal;

    [Header("Season Reports")]
    [SerializeField] private GameObject SeasonReport;

    private bool isSeasonFinished = false;

    void Awake()
    {
        CharacterButton.onClick.AddListener(ShowCharacter);
        PatchNoteButton.onClick.AddListener(ShowPatchNote);
        GraphButton.onClick.AddListener(ShowDataGraph);
        UnlockButton.onClick.AddListener(ShowUnlock);
        GoalButton.onClick.AddListener(ShowGoals);
        ReportButton.onClick.AddListener(ShowSeasonReports);
    }

    public void SetSeasonFinish(bool value)
    {
        isSeasonFinished = value;
    }

    private void RemoveAll()
    {
        CharacterTable.SetActive(false);
        Inspector.SetActive(false);
        CharacterPreview.SetActive(false);
        Graphs.SetActive(false);
        UnlockUI.SetActive(false);
        PatchNotes.SetActive(false);
        DeveloperGoal.SetActive(false);
        BottomDisplay.SetActive(false);
        SeasonReport.SetActive(false);
    }

    public void ShowCharacter()
    {
        RemoveAll();
        if (!isSeasonFinished) SeasonManager.Instance.CheckSeasonFinished();

        CharacterTable.SetActive(true);
        Inspector.SetActive(true);
        BottomDisplay.SetActive(true);
        CharacterPreview.SetActive(true);
    }

    public void ShowPatchNote()
    {
        RemoveAll();
        if (!isSeasonFinished) SeasonManager.Instance.CheckSeasonFinished();

        PatchNotes.SetActive(true);
    }

    public void ShowDataGraph()
    {
        RemoveAll();
        if (!isSeasonFinished) SeasonManager.Instance.CheckSeasonFinished();
        Graphs.SetActive(true);
    }

    public void ShowUnlock()
    {
        RemoveAll();
        if (!isSeasonFinished) SeasonManager.Instance.CheckSeasonFinished();

        UnlockUI.SetActive(true);
        OnUnlockUIOpen?.Invoke();
    }

    public void ShowGoals()
    {
        RemoveAll();
        if (!isSeasonFinished) SeasonManager.Instance.CheckSeasonFinished();

        DeveloperGoal.GetComponent<DeveloperGoalUI>().RefreshUI();
        DeveloperGoal.SetActive(true);
    }

    public void ShowSeasonReports()
    {
        RemoveAll();
        SeasonReport.SetActive(true);
    }
}

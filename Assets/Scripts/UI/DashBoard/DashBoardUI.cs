using UnityEngine;
using UnityEngine.UI;

public class DashBoardUI : MonoBehaviour
{
    [SerializeField] private Button CharacterButton;
    [SerializeField] private Button PatchNoteButton;
    [SerializeField] private Button SeasonReportButton;
    [SerializeField] private Button UnlockButton;
    [SerializeField] private Button GoalButton;
    [SerializeField] private Button ReportButton;

    [Header("Character Table")]
    [SerializeField] private GameObject CharacterTable;
    [SerializeField] private GameObject Inspector;
    [SerializeField] private GameObject BottomDisplay;

    [Header("Patch Notes")]
    [SerializeField] private GameObject PatchNotes;

    [Header("Events")]
    [SerializeField] private GameObject Events;

    [Header("Unlock System")]
    [SerializeField] private GameObject UnlockUI;

    [Header("Developer Goals")]
    [SerializeField] private GameObject DeveloperGoal;

    [Header("Season Reports")]
    [SerializeField] private GameObject SeasonReport;

    void Awake()
    {
        CharacterButton.onClick.AddListener(ShowCharacter);
        PatchNoteButton.onClick.AddListener(ShowPatchNote);
        UnlockButton.onClick.AddListener(ShowUnlock);
        GoalButton.onClick.AddListener(ShowGoals);
        ReportButton.onClick.AddListener(ShowSeasonReports);
    }

    private void RemoveAll()
    {
        CharacterTable.SetActive(false);
        Inspector.SetActive(false);
        Events.SetActive(false);
        UnlockUI.SetActive(false);
        PatchNotes.SetActive(false);
        DeveloperGoal.SetActive(false);
        BottomDisplay.SetActive(false);
        SeasonReport.SetActive(false);
    }

    public void ShowCharacter()
    {
        RemoveAll();

        CharacterTable.SetActive(true);
        Inspector.SetActive(true);
        BottomDisplay.SetActive(true);
    }

    public void ShowPatchNote()
    {
        RemoveAll();
        PatchNotes.SetActive(true);
    }

    public void ShowUnlock()
    {
        RemoveAll();
        UnlockUI.SetActive(true);
    }

    public void ShowGoals()
    {
        RemoveAll();
        DeveloperGoal.GetComponent<DeveloperGoalUI>().RefreshUI();
        DeveloperGoal.SetActive(true);
    }

    public void ShowSeasonReports()
    {
        RemoveAll();
        SeasonReport.SetActive(true);
    }
}

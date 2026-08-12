using UnityEngine;
using UnityEngine.UI;

public class DashBoardUI : MonoBehaviour
{
    [SerializeField] private Button CharacterButton;
    [SerializeField] private Button PatchNoteButton;
    [SerializeField] private Button SeasonReportButton;
    [SerializeField] private Button UnlockButton;
    [SerializeField] private Button GoalButton;

    [Header("Character Table")]
    [SerializeField] private GameObject CharacterTable;
    [SerializeField] private GameObject Inspector;
    [SerializeField] private GameObject BottomDisplay;

    [Header("Patch Notes")]
    [SerializeField] private GameObject PatchNotes;

    [Header("Season Reports")]
    [SerializeField] private GameObject SeasonReports;

    [Header("Unlock System")]
    [SerializeField] private GameObject UnlockUI;

    [Header("Developer Goals")]
    [SerializeField] private GameObject DeveloperGoal;

    void Awake()
    {
        CharacterButton.onClick.AddListener(ShowCharacter);
        PatchNoteButton.onClick.AddListener(ShowPatchNote);
        UnlockButton.onClick.AddListener(ShowUnlock);
        GoalButton.onClick.AddListener(ShowGoals);
    }

    private void RemoveAll()
    {
        CharacterTable.SetActive(false);
        Inspector.SetActive(false);
        SeasonReports.SetActive(false);
        UnlockUI.SetActive(false);
        PatchNotes.SetActive(false);
        DeveloperGoal.SetActive(false);
        BottomDisplay.SetActive(false);
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
}

using UnityEngine;
using UnityEngine.UI;

public class DashBoardUI : MonoBehaviour
{
    [SerializeField] private Button CharacterButton;
    [SerializeField] private Button PatchNoteButton;
    [SerializeField] private Button SeasonReportButton;
    [SerializeField] private Button UnlockButton;
    [SerializeField] private Button GoalButton;

    [Header("CharacterObject")]
    [SerializeField] private GameObject CharacterTable;
    [SerializeField] private GameObject Inspector;
    [SerializeField] private GameObject DeveloperGoal;

    void Awake()
    {
        CharacterButton.onClick.AddListener(ShowCharacter);
        GoalButton.onClick.AddListener(ShowGoals);
    }

    private void RemoveAll()
    {
        CharacterTable.SetActive(false);
        Inspector.SetActive(false);
        DeveloperGoal.SetActive(false);
    }

    public void ShowCharacter()
    {
        RemoveAll();

        CharacterTable.SetActive(true);
        Inspector.SetActive(true);
    }

    public void ShowGoals()
    {
        RemoveAll();
        DeveloperGoal.GetComponent<DeveloperGoalUI>().RefreshUI();
        DeveloperGoal.SetActive(true);
    }
}

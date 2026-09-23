using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperGoalUI : MonoBehaviour
{
    [SerializeField] private DeveloperGoalItemUI[] goalsUI;
    [SerializeField] private Button[] GoalSelectButtons;

    public GameObject GoalUIPrefab;

    [SerializeField] private TMP_Text SeasonText;
    [SerializeField] private TMP_Text TitleText;
    [SerializeField] private TMP_Text DRReward;
    [SerializeField] private TMP_Text TPReward;
    [SerializeField] private TMP_Text RerollCost;
    [SerializeField] private TMP_Text RerollCostValue;
    [SerializeField] private Button ChangeButton;
    [SerializeField] private Button ConfirmButton;
    [SerializeField] private GameObject GoalAlert;

    private int selectedIndex = -1;

    public void Initialize(List<DeveloperGoal> goals, GoalManager goalManager)
    {
        ChangeButton.onClick.AddListener(goalManager.ChangeGoals);
        ConfirmButton.onClick.AddListener(goalManager.ConfirmGoals);

        for (int i = 0; i < GoalSelectButtons.Length; i++)
        {
            int index = i;

            GoalSelectButtons[i].onClick.AddListener(() => goalManager.SelectGoal(index));
        }

        SetGoals(goals);
    }

    public void SetGoals(List<DeveloperGoal> goals)
    {
        selectedIndex = -1;

        for (int i = 0; i < goalsUI.Length; i++)
        {
            if (i < goals.Count)
            {
                goalsUI[i].Initialize(goals[i]);
            }
        }

        ConfirmButton.interactable = false;

        RefreshUI();
    }

    public void SetGoals(DeveloperGoal goal, int index)
    {
        if (index < 0 || index >= goalsUI.Length)
            return;

        goalsUI[index].Initialize(goal);
    }

    public void SetSelectedGoal(int index)
    {
        if (index < 0 || index >= goalsUI.Length)
            return;

        selectedIndex = index;
        ConfirmButton.interactable = true;
        TitleText.text = goalsUI[index].Goal.Title;

        RefreshSelectionUI();
    }

    private void RefreshSelectionUI()
    {
        for (int i = 0; i < GoalSelectButtons.Length; i++)
        {
            GoalSelectButtons[i].interactable = i != selectedIndex;
        }
    }

    public void RefreshUI()
    {
        int developerReward = 0;
        int trustPoint = 0;

        foreach (DeveloperGoalItemUI goal in goalsUI)
        {
            if (goal.Goal == null)
                continue;

            if (goal.Goal.IsComplete)
            {
                developerReward += goal.Goal.Reward.DevelopResource;
                trustPoint += goal.Goal.Reward.TrustPoint;
            }

            goal.ReflectProgrss();
        }

        SeasonText.text = $"시즌 {SeasonManager.Instance.CurrentSeason} - {SeasonManager.Instance.CurrentSubSeason}";
        DRReward.text = $"+{developerReward}";
        TPReward.text = $"+{trustPoint}";
    }

    public void SetRerollCostValue(int value)
    {
        RerollCostValue.text = value.ToString();
    }

    public void ShowAlert(bool show)
    {
        GoalAlert.SetActive(show);
    }
}
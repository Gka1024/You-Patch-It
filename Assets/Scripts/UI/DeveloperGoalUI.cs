using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperGoalUI : MonoBehaviour
{
    [SerializeField] private DeveloperGoalItemUI[] goalsUI;
    public GameObject GoalUIPrefab;

    [SerializeField] private TMP_Text SeasonText;
    [SerializeField] private TMP_Text[] TitleText;
    [SerializeField] private TMP_Text DRReward;
    [SerializeField] private TMP_Text TPReward;
    [SerializeField] private Button ChangeButton;

    public void Initialize(List<DeveloperGoal> goals)
    {
        for (int i = 0; i < goals.Count; i++)
        {
            goalsUI[i].Initialize(goals[i]);
        }
        ChangeButton.onClick.AddListener(RefreshUI);
    }

    public void RefreshUI()
    {
        int index = 0;
        int DeveloperReward = 0;
        int TrustPoint = 0;

        foreach (DeveloperGoalItemUI goal in goalsUI)
        {
            TitleText[index++].text = goal.Goal.Title;

            if (goal.Goal.IsComplete)
            {
                DeveloperReward += goal.Goal.Reward.DevelopResource;
                TrustPoint += goal.Goal.Reward.TrustPoint;
            }

            goal.ReflectProgrss();
        }

        SeasonText.text = $"시즌 {SeasonManager.Instance.CurrentSeason} - {SeasonManager.Instance.CurrentSubSeason}";
        DRReward.text = $"+{DeveloperReward}";
        TPReward.text = $"+{TrustPoint}";


    }

}
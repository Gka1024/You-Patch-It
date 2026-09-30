using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomGoalPreviewUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goalTitle;
    [SerializeField] private TMP_Text goalDescription;

    public Button MoveToButton;

    public void Initialize(GoalManager manager)
    {
        MoveToButton.onClick.AddListener(UIManager.Instance.dashBoardUI.ShowGoals);
    }

    public void Reset()
    {
        goalTitle.text = "";
    }

    public void SetText(DeveloperGoal currentGoal)
    {
        if (currentGoal == null) return;

        Reset();

        goalTitle.text = currentGoal.Title;
        goalDescription.text = currentGoal.Description;
    }
}

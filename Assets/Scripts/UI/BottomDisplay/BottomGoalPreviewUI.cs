using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomGoalPreviewUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goalTitle;

    public Button RerollButton;
    public Button ConfirmButton;

    public void Initialize(GoalManager manager)
    {
        RerollButton.onClick.AddListener(manager.ChangeGoals);
        ConfirmButton.onClick.AddListener(manager.ConfirmGoals);
    }

    public void Reset()
    {
        goalTitle.text = "";
    }

    public void SetText(DeveloperGoal currentGoals)
    {
        if (currentGoals == null) return;

        Reset();

        goalTitle.text = currentGoals.Title;
    }
}

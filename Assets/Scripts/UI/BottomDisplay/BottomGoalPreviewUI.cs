using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomGoalPreviewUI : MonoBehaviour
{
    [SerializeField] private TMP_Text[] goalTitle;

    public Button RerollButton;
    public Button ConfirmButton;

    public void Initialize(GoalManager manager)
    {
        RerollButton.onClick.AddListener(manager.ChangeGoals);
        ConfirmButton.onClick.AddListener(manager.ConfirmGoals);
    }

    public void Reset()
    {
        foreach (TMP_Text text in goalTitle)
        {
            text.text = " - ";
        }
    }

    public void SetText(List<DeveloperGoal> currentGoals)
    {
        Reset();

        for (int i = 0; i < currentGoals.Count; i++)
        {
            goalTitle[i].text = currentGoals[i].Title + " : " + currentGoals[i].Description;
        }
    }
}

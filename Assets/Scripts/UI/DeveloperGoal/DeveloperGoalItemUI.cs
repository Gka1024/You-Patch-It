using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperGoalItemUI : MonoBehaviour
{
    public DeveloperGoal Goal { get; private set; }

    [SerializeField] private TMP_Text Title;
    [SerializeField] private TMP_Text Description;
    [SerializeField] private TMP_Text CurStatus;
    [SerializeField] private TMP_Text Reward;
    [SerializeField] private TMP_Text DevelopResource;
    [SerializeField] private TMP_Text DRReward;
    [SerializeField] private TMP_Text TrustPoint;
    [SerializeField] private TMP_Text TPReward;

    [SerializeField] private RectTransform Gauge;
    private float maxWidth;

    public void Initialize(DeveloperGoal goal)
    {
        this.Goal = goal;

        Title.text = goal.Title;
        Description.text = goal.Description;

        DRReward.text = goal.Reward.DevelopResource.ToString();
        TPReward.text = goal.Reward.TrustPoint.ToString();
        maxWidth = Gauge.sizeDelta.x;
    }

    public void ReflectProgrss()
    {
        AdjustGauge(Goal.GetCurrentProgress());
    }

    private void AdjustGauge(float value, float max = 1)
    {
        float progress = Mathf.Clamp01(value / max);

        Vector2 size = Gauge.sizeDelta;
        size.x = maxWidth * progress;
        Gauge.sizeDelta = size;
    }
}

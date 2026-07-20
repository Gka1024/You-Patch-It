using TMPro;
using UnityEngine;

public class DeveloperGoalItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text Title;
    [SerializeField] private TMP_Text Description;
    [SerializeField] private TMP_Text CurStatus;
    [SerializeField] private TMP_Text Reward;
    [SerializeField] private TMP_Text DevelopResource;
    [SerializeField] private TMP_Text DRReward;
    [SerializeField] private TMP_Text TrustPoint;
    [SerializeField] private TMP_Text TPReward;

    public DeveloperGoal Goal {get; private set;}

    public void Initialize(DeveloperGoal goal)
    {
        this.Goal = goal;

        Title.text = goal.Title;
        Description.text = goal.Description;

        DRReward.text = goal.DevelopResourceReward.ToString();
        TPReward.text = goal.TrustPointReward.ToString();
    }
}

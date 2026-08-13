using UnityEngine;
using UnityEngine.UI;

public class BottomDisplayUI : MonoBehaviour
{
    public static BottomDisplayUI Instance;

    public Button userReactionButton;
    public Button skillDescriptionButton;
    public Button goalPreviewButton;

    public BottomUserReactionUI UserReaction;
    public BottomSkillDescriptionUI SkillDescription;
    public BottomGoalPreviewUI GoalPreview;

    public GameObject UserReactionObject;
    public GameObject SkillDescriptionObject;
    public GameObject GoalPreviewObject;

    void Awake()
    {
        Instance = this;

        UserReaction = UserReactionObject.GetComponent<BottomUserReactionUI>();
        SkillDescription = SkillDescriptionObject.GetComponent<BottomSkillDescriptionUI>();
        GoalPreview = GoalPreviewObject.GetComponent<BottomGoalPreviewUI>();

        userReactionButton.onClick.AddListener(ShowReaction);
        skillDescriptionButton.onClick.AddListener(ShowDescription);
        goalPreviewButton.onClick.AddListener(ShowPreview);
    }

    private void Removeall()
    {
        UserReactionObject.SetActive(false);
        SkillDescriptionObject.SetActive(false);
        GoalPreviewObject.SetActive(false);
    }

    private void ShowReaction()
    {
        Removeall();
        UserReactionObject.SetActive(true);
    }

    private void ShowDescription()
    {
        Removeall();
        SkillDescriptionObject.SetActive(true);
    }

    private void ShowPreview()
    {
        Removeall();
        GoalPreviewObject.SetActive(true);
    }
}
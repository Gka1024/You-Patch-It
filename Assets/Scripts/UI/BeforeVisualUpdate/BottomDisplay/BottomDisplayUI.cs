using UnityEngine;
using UnityEngine.UI;

public class BottomDisplayUI : MonoBehaviour
{
    public static BottomDisplayUI Instance;

    public Button userReactionButton;
    public Button skillDescriptionButton;
    public Button goalPreviewButton;
    public Button removePatchReasonButton;

    public BottomUserReactionUI UserReaction;
    public BottomSkillDescriptionUI SkillDescription;
    public BottomGoalPreviewUI GoalPreview;

    public GameObject UserReactionObject;
    public GameObject SkillDescriptionObject;
    public GameObject GoalPreviewObject;
    public GameObject PatchReasonObject;

    void Awake()
    {
        Instance = this;

        UserReaction = UserReactionObject.GetComponent<BottomUserReactionUI>();
        SkillDescription = SkillDescriptionObject.GetComponent<BottomSkillDescriptionUI>();
        GoalPreview = GoalPreviewObject.GetComponent<BottomGoalPreviewUI>();

        userReactionButton.onClick.AddListener(ShowReaction);
        skillDescriptionButton.onClick.AddListener(ShowDescription);
        goalPreviewButton.onClick.AddListener(ShowPreview);
        removePatchReasonButton.onClick.AddListener(RemovePatchReason);
    }

    private void Removeall()
    {
        UserReactionObject.SetActive(false);
        SkillDescriptionObject.SetActive(false);
        GoalPreviewObject.SetActive(false);
        PatchReasonObject.SetActive(false);
    }

    public void ShowReaction()
    {
        Removeall();
        UserReactionObject.SetActive(true);
    }

    public void ShowDescription()
    {
        Removeall();
        SkillDescriptionObject.SetActive(true);
    }

    public void ShowPreview()
    {
        Removeall();
        GoalPreviewObject.SetActive(true);
    }

    public void RemovePatchReason()
    {
        PatchReasonObject.SetActive(false);
    }
}
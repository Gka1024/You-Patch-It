using UnityEngine;
using UnityEngine.UI;

public class BottomDisplayUI : MonoBehaviour
{
    public Button userReactionButton;
    public Button skillDescriptionButton;
    public Button goalPreviewButton;

    public GameObject UserReactionObject;
    public GameObject SkillDescriptionObject;
    public GameObject GoalPreviewObject;

    void Awake()
    {
        userReactionButton.onClick.AddListener(ShowReaction);
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
    }

    private void ShowDescription()
    {
        Removeall();
    }

    private void ShowPreview()
    {
        Removeall();
    }
}
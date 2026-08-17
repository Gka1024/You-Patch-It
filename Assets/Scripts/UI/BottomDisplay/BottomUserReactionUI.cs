using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomUserReactionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text[] reactions;
    [SerializeField] private Button button;
    [SerializeField] private Toggle toggle;
    public Toggle Toggle => toggle;
    public bool IsOn => toggle.isOn;

    void Awake()
    {
        button.onClick.AddListener(Refresh);
        ResetToggle();
    }

    public void ResetToggle()
    {
        toggle.SetIsOnWithoutNotify(false);
    }

    private void Refresh()
    {
        if(SeasonManager.Instance.CurrentSeason == 1 && SeasonManager.Instance.CurrentSubSeason == 1) return;

        for (int i = 0; i < reactions.Length; i++)
        {
            RuntimeCharacter character = RuntimeCharacterManager.Instance.GetRandomCharacter();
            UserReactionCategory category = UserReactionManager.Instance.GetCategory(AnalysisManager.Instance.GetTier(character));
            SetText(reactions[i], UserReactionManager.Instance.GetReaction(character, category, IsOn));
        }
    }

    private void SetText(TMP_Text text, string content)
    {
        text.text = content;
    }



}


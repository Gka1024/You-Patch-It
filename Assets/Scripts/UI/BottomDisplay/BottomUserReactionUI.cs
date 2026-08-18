using System.Collections;
using System.Collections.Generic;
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

    public bool LoopOn { get; private set; }
    private Coroutine autoRefreshCoroutine;

    void Awake()
    {
        LoopOn = false;
        button.onClick.AddListener(Refresh);
        ResetToggle();
    }

    private void OnEnable()
    {
        autoRefreshCoroutine = StartCoroutine(AutoRefresh());
    }

    private void OnDisable()
    {
        if (autoRefreshCoroutine != null)
        {
            StopCoroutine(autoRefreshCoroutine);
            autoRefreshCoroutine = null;
        }
    }

    public void ResetToggle()
    {
        toggle.SetIsOnWithoutNotify(false);
    }

    private void Refresh()
    {
        if (SeasonManager.Instance.CurrentSeason == 1 && SeasonManager.Instance.CurrentSubSeason == 1) return;

        HashSet<int> usedReactionIds = new();

        for (int i = 0; i < reactions.Length; i++)
        {
            RuntimeCharacter character = RuntimeCharacterManager.Instance.GetRandomCharacter();

            UserReactionCategory category = UserReactionManager.Instance.GetCategory(AnalysisManager.Instance.GetTier(character));

            string reaction = UserReactionManager.Instance.GetReaction(character, category, IsOn, usedReactionIds);

            SetText(reactions[i], reaction);
        }
    }

    private void SetText(TMP_Text text, string content)
    {
        text.text = content;
    }

    public void TurnOnLoop()
    {
        LoopOn = true;
    }

    private IEnumerator AutoRefresh()
    {
        while (LoopOn)
        {
            Refresh();
            yield return new WaitForSeconds(3f);
        }
    }

}


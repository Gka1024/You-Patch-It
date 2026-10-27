using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomUserReactionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text[] reactions;
    [SerializeField] private Button button;
    [SerializeField] private Toggle toggle;

    [Header("Animation")]
    [SerializeField] private float moveDuration = 0.35f;
    [SerializeField] private float staggerDelay = 0.04f;
    [SerializeField] private float rowSpacing = 30f;

    public Toggle Toggle => toggle;
    public bool IsOn => toggle.isOn;
    public bool LoopOn { get; private set; }

    private Coroutine autoRefreshCoroutine;
    private Vector2[] originalPositions;
    private Sequence currentSequence;
    private bool isAnimating;

    private void Awake()
    {
        LoopOn = false;
        button.onClick.AddListener(Refresh);
        ResetToggle();
    }

    private void Start()
    {
        CacheOriginalPositions();
    }

    private void OnEnable()
    {
        if (LoopOn)
            StartAutoRefresh();
    }

    private void OnDisable()
    {
        StopAutoRefresh();

        if (currentSequence != null)
        {
            currentSequence.Kill();
            currentSequence = null;
        }

        isAnimating = false;
        RestorePositions();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(Refresh);
    }

    private void CacheOriginalPositions()
    {
        originalPositions = new Vector2[reactions.Length];

        for (int i = 0; i < reactions.Length; i++)
        {
            originalPositions[i] = reactions[i].rectTransform.anchoredPosition;
        }

        // 실제 UI 배치 간격을 우선 사용
        if (reactions.Length > 1)
        {
            float actualSpacing = Mathf.Abs(originalPositions[0].y - originalPositions[1].y);

            if (actualSpacing > 0f)
                rowSpacing = actualSpacing;
        }
    }

    public void ResetToggle()
    {
        toggle.SetIsOnWithoutNotify(false);
    }

    public void SelfRefresh()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (isAnimating)
            return;

        if (SeasonManager.Instance.CurrentSeason == 1 && SeasonManager.Instance.CurrentSubSeason == 1)
            return;

        HashSet<int> usedReactionIds = new();
        List<string> newReactions = new();

        for (int i = 0; i < reactions.Length; i++)
        {
            RuntimeCharacter character = RuntimeCharacterManager.Instance.GetRandomCharacter();

            UserReactionCategory category = UserReactionManager.Instance.GetCategory(AnalysisManager.Instance.GetTier(character));

            string reaction = UserReactionManager.Instance.GetReaction(character, category, IsOn, usedReactionIds);

            newReactions.Add(reaction);
        }

        PlayReactionAnimation(newReactions);
    }


    private void PlayReactionAnimation(List<string> newReactions)
    {
        if (reactions.Length == 0 || originalPositions == null)
            return;

        isAnimating = true;

        currentSequence?.Kill();
        currentSequence = DOTween.Sequence();

        int lastIndex = reactions.Length - 1;

        // 기존 텍스트 내용을 저장한다.
        string[] oldReactions = new string[reactions.Length];

        for (int i = 0; i < reactions.Length; i++)
        {
            oldReactions[i] = reactions[i].text;
        }

        // 가장 위의 텍스트는 위로 이동하면서 사라진다.
        TMP_Text firstReaction = reactions[0];

        currentSequence.Join(firstReaction.rectTransform.DOAnchorPos(originalPositions[0] + Vector2.up * rowSpacing, moveDuration).SetEase(Ease.OutCubic));
        currentSequence.Join(firstReaction.DOFade(0f, moveDuration));

        // 나머지 텍스트는 한 칸씩 위로 이동한다.
        for (int i = 1; i < reactions.Length; i++)
        {
            RectTransform rect = reactions[i].rectTransform;
            Vector2 targetPosition = originalPositions[i - 1];

            currentSequence.Join(rect.DOAnchorPos(targetPosition, moveDuration).SetEase(Ease.OutCubic));
        }

        // 기존 텍스트가 이동한 직후, 각 슬롯의 텍스트를 갱신한다.
        currentSequence.AppendCallback(() =>
        {
            for (int i = 0; i < lastIndex; i++)
            {
                reactions[i].text = oldReactions[i + 1];
                reactions[i].rectTransform.anchoredPosition = originalPositions[i];
            }

            // 새로운 텍스트를 가장 아래에 배치한다.
            TMP_Text lastReaction = reactions[lastIndex];

            lastReaction.text = newReactions[lastIndex];
            lastReaction.rectTransform.anchoredPosition = originalPositions[lastIndex] + Vector2.down * rowSpacing;

            Color color = lastReaction.color;
            color.a = 0f;
            lastReaction.color = color;

            // 사라졌던 첫 번째 텍스트의 위치와 투명도를 복구한다.
            firstReaction.rectTransform.anchoredPosition = originalPositions[0];

            Color firstColor = firstReaction.color;
            firstColor.a = 1f;
            firstReaction.color = firstColor;
        });

        // 새 텍스트가 아래에서 올라오며 나타난다.
        TMP_Text newReaction = reactions[lastIndex];

        currentSequence.Append(newReaction.rectTransform.DOAnchorPos(originalPositions[lastIndex], moveDuration).SetEase(Ease.OutCubic));
        currentSequence.Join(newReaction.DOFade(1f, moveDuration));

        currentSequence.OnComplete(() =>
        {
            for (int i = 0; i < reactions.Length; i++)
            {
                reactions[i].rectTransform.anchoredPosition = originalPositions[i];
            }

            isAnimating = false;
            currentSequence = null;
        });
    }

    private void RestorePositions()
    {
        if (originalPositions == null)
            return;

        for (int i = 0; i < reactions.Length; i++)
        {
            reactions[i].rectTransform.anchoredPosition = originalPositions[i];
        }
    }

    public void TurnOnLoop()
    {
        LoopOn = true;

        Refresh();
        StartAutoRefresh();
    }

    public void TurnOffLoop()
    {
        LoopOn = false;
        StopAutoRefresh();
    }

    private void StartAutoRefresh()
    {
        if (!isActiveAndEnabled || autoRefreshCoroutine != null)
            return;

        Debug.Log("Auto Refresh Coroutine Initialized");
        autoRefreshCoroutine = StartCoroutine(AutoRefresh());
    }

    private void StopAutoRefresh()
    {
        if (autoRefreshCoroutine == null)
            return;

        StopCoroutine(autoRefreshCoroutine);
        autoRefreshCoroutine = null;
    }

    private IEnumerator AutoRefresh()
    {
        Debug.Log("AutoRefresh 시작");

        Refresh();

        while (LoopOn)
        {
            yield return new WaitForSecondsRealtime(5f);

            if (LoopOn)
                Refresh();
        }

        autoRefreshCoroutine = null;
        Debug.Log("AutoRefresh 종료");
    }
}
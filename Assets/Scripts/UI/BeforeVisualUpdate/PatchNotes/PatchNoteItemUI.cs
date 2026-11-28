using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatchNoteItemUI : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private Button itemButton;
    [SerializeField] private TMP_Text arrow;

    [SerializeField] private TMP_Text currentSeasonText;
    [SerializeField] private TMP_Text seasonWinrate;
    [SerializeField] private TMP_Text seasonPickrate;
    [SerializeField] private TMP_Text patchCount;

    [Header("Body")]
    [SerializeField] private GameObject body;

    [SerializeField] private GameObject specificPatch;
    [SerializeField] private Transform specificPatchParent;

    [SerializeField] private TMP_Text patchDescriptionText;

    [Header("Layout")]
    [SerializeField] private LayoutElement layoutElement;

    public float CurrentHeight => layoutElement.preferredHeight;
    public RectTransform Rect => transform as RectTransform;

    [SerializeField] private float closedHeight = 80f;
    [SerializeField] private float bodyHeaderHeight = 40f;
    [SerializeField] private float patchItemHeight = 50f;
    [SerializeField] private float bodyPadding = 20f;

    [SerializeField] private float animationTime = 0.2f;

    private Coroutine animationCoroutine;

    [Header("Other")]
    private PatchHistory history;

    public event System.Action<PatchNoteItemUI> OnHeightChanged;
    public event System.Action<PatchNoteItemUI> OnClicked;

    private bool opened;
    public bool IsOpened => opened;

    private void Awake()
    {
        itemButton.onClick.AddListener(Toggle);

        if (layoutElement == null)
            layoutElement = GetComponent<LayoutElement>();

        layoutElement.preferredHeight = closedHeight;

        body.SetActive(false);
    }

    public void Initialize(PatchHistory history)
    {
        this.history = history;

        Refresh();
        SetOpen(false, true);
    }

    public void Refresh()
    {
        if (history == null)
            return;

        currentSeasonText.text = $"시즌 {history.Season}-{history.SubSeason}";
        seasonWinrate.text = $"승률 : {history.Winrate:F1}%";
        seasonPickrate.text = $"픽률 : {history.Pickrate:F1}%";

        //----------------------------------------
        // 기존 생성 삭제
        //----------------------------------------

        foreach (Transform child in specificPatchParent)
            Destroy(child.gameObject);

        //----------------------------------------
        // Patch
        //----------------------------------------

        int statCount = 0;

        foreach (CharacterStatType stat in System.Enum.GetValues(typeof(CharacterStatType)))
        {
            if (!history.TryGetStatPatch(stat, out float before, out float after))
                continue;

            if (Mathf.Approximately(before, after))
                continue;

            Instantiate(specificPatch, specificPatchParent)
                .GetComponent<SpecificPatchUI>()
                .Initialize(stat, before, after);

            statCount++;
        }

        patchCount.text = $"패치 수 : {statCount} 개";

        //----------------------------------------
        // Patch Description
        //----------------------------------------

        patchDescriptionText.text = history.PatchDescription;

        //----------------------------------------
        // Layout 갱신
        //----------------------------------------

        LayoutRebuilder.ForceRebuildLayoutImmediate(patchDescriptionText.rectTransform);

        if (opened)
            SetOpen(true, true);
    }

    public void Toggle()
    {
        OnClicked?.Invoke(this);
    }

    public void SetOpen(bool value, bool instant = false)
    {
        opened = value;

        arrow.text = opened ? "▼" : "▶";

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (instant)
        {
            body.SetActive(opened);

            layoutElement.preferredHeight = opened ? GetOpenHeight() : closedHeight;

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent as RectTransform);

            OnHeightChanged?.Invoke(this);

            return;
        }

        animationCoroutine = StartCoroutine(Animate(opened));
    }

    private float GetOpenHeight()
    {
        int patchCount = specificPatchParent.childCount;

        float patchHeight = patchCount * patchItemHeight;

        float descriptionHeight = 0f;

        if (!string.IsNullOrWhiteSpace(patchDescriptionText.text))
        {
            descriptionHeight = patchDescriptionText.preferredHeight;
        }

        float bodyHeight = bodyHeaderHeight + Mathf.Max(patchHeight, descriptionHeight) + bodyPadding;

        return closedHeight + bodyHeight;
    }

    private IEnumerator Animate(bool open)
    {
        float startHeight = layoutElement.preferredHeight;
        float targetHeight = open ? GetOpenHeight() : closedHeight;

        body.SetActive(open);

        float elapsed = 0f;

        while (elapsed < animationTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / animationTime);
            t = 1f - Mathf.Pow(1f - t, 3f);

            layoutElement.preferredHeight = Mathf.Lerp(startHeight, targetHeight, t);

            OnHeightChanged?.Invoke(this);

            yield return null;
        }

        layoutElement.preferredHeight = targetHeight;

        OnHeightChanged?.Invoke(this);

        animationCoroutine = null;
    }
}
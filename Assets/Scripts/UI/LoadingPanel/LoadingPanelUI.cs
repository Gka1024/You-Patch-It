using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanelUI : MonoBehaviour
{
    [Header("Loading Text")]
    [SerializeField] private TMP_Text loadingDots;
    [SerializeField] private float dotInterval = 0.4f;
    [SerializeField] private int maxDotCount = 3;

    [Header("Tip")]
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private Button tipButton;
    [SerializeField] private TextAsset tipJson;
    [SerializeField] private float tipChangeInterval = 5f;
    [SerializeField] private float tipFadeDuration = 0.2f;

    [Header("Progress")]
    [SerializeField] private Image progressFill;
    [SerializeField] private float progressTweenDuration = 0.2f;
    [SerializeField] private TMP_Text currentProgressText;

    private Coroutine dotCoroutine;
    private Coroutine tipCoroutine;

    private string[] tips;
    private int currentTipIndex = -1;

    private Tween tipTween;
    private Tween progressTween;

    [System.Serializable]
    private class TipData
    {
        public string[] tips;
    }

    private void Awake()
    {
        LoadTips();

        if (tipButton != null)
            tipButton.onClick.AddListener(RefreshTip);
    }

    private void OnEnable()
    {
        ResetProgress();

        RefreshTip();

        dotCoroutine = StartCoroutine(AnimateDots());
        tipCoroutine = StartCoroutine(AnimateTips());
    }

    private void OnDisable()
    {
        if (dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
            dotCoroutine = null;
        }

        if (tipCoroutine != null)
        {
            StopCoroutine(tipCoroutine);
            tipCoroutine = null;
        }

        tipTween?.Kill();
        progressTween?.Kill();
    }

    private void OnDestroy()
    {
        if (tipButton != null)
            tipButton.onClick.RemoveListener(RefreshTip);
    }

    private void LoadTips()
    {
        if (tipJson == null)
        {
            Debug.LogWarning("LoadingPanelUI: Tip JSON is missing.");
            tips = new string[0];
            return;
        }

        TipData data = JsonUtility.FromJson<TipData>(tipJson.text);

        if (data == null || data.tips == null)
        {
            Debug.LogWarning("LoadingPanelUI: Failed to load tip data.");
            tips = new string[0];
            return;
        }

        tips = data.tips;
    }

    private IEnumerator AnimateDots()
    {
        int dotCount = 1;

        while (true)
        {
            loadingDots.text = "Now Loading " + new string('.', dotCount);

            dotCount++;

            if (dotCount > maxDotCount)
                dotCount = 1;

            yield return new WaitForSecondsRealtime(dotInterval);
        }
    }

    private IEnumerator AnimateTips()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(tipChangeInterval);
            RefreshTip();
        }
    }

    private void RefreshTip()
    {
        if (tips == null || tips.Length == 0 || tipText == null)
            return;

        int nextIndex = Random.Range(0, tips.Length);

        if (tips.Length > 1)
        {
            while (nextIndex == currentTipIndex)
                nextIndex = Random.Range(0, tips.Length);
        }

        currentTipIndex = nextIndex;

        tipTween?.Kill();

        tipTween = DOTween.Sequence()
            .Append(tipText.DOFade(0f, tipFadeDuration))
            .AppendCallback(() => tipText.text = tips[currentTipIndex])
            .Append(tipText.DOFade(1f, tipFadeDuration));
    }

    public void SetProgress(float progress)
    {
        if (progressFill == null)
            return;

        progress = Mathf.Clamp01(progress);

        progressTween?.Kill();

        progressTween = progressFill.DOFillAmount(progress, progressTweenDuration);
        currentProgressText.text = $"{progress * 100:F1} %";
    }

    public void SetProgress(int completed, int total)
    {
        if (total <= 0)
        {
            SetProgress(0f);
            return;
        }

        SetProgress((float)completed / total);
    }

    private void ResetProgress()
    {
        progressTween?.Kill();

        if (progressFill != null)
            progressFill.fillAmount = 0f;
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text winRateText;
    [SerializeField] private TMP_Text pickRateText;
    [SerializeField] private TMP_Text tierText;
    [SerializeField] private TMP_Text banRateText;
    [SerializeField] private TMP_Text DPSText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text livetimeText;

    [SerializeField] private Image Symbols;
    [SerializeField] private Image PinImage;
    [SerializeField] private CharacterRowClickChecker RowClickChecker;
    [SerializeField] private GameObject backgroundImage;

    private RuntimeCharacter runtimeCharacter;
    private CharacterTableUI tableUI;

    private bool isInitialized;
    public bool isNewCharacter;

    private const int UnlockShowTier = 1021;
    private const int UnlockShowBan = 1022;
    private const int UnlockShowLivetime = 1023;
    private const int UnlockShowDPS = 1024;

    public RuntimeCharacter RuntimeCharacter => runtimeCharacter;

    /// <summary>
    /// 행을 처음 생성했을 때 한 번 호출한다.
    /// </summary>
    public void Initialize(CharacterTableUI tableUI)
    {
        isNewCharacter = true;
        this.tableUI = tableUI;

        if (!isInitialized)
        {
            RowClickChecker.Initialize(this);

            if (UnlockManager.Instance != null)
                UnlockManager.Instance.OnUnlockChanged += Refresh;

            isInitialized = true;
        }
    }

    /// <summary>
    /// 기존 행에 표시할 캐릭터를 새로 연결한다.
    /// </summary>
    public void SetCharacter(RuntimeCharacter character, int rank)
    {
        if (runtimeCharacter != character)
        {
            if (runtimeCharacter != null)
                runtimeCharacter.OnStatChanged -= Refresh;

            runtimeCharacter = character;

            if (runtimeCharacter != null)
                runtimeCharacter.OnStatChanged += Refresh;
        }

        UpdateRank(rank);

        if (runtimeCharacter == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Symbols.sprite = tableUI.GetSymbolSprite(runtimeCharacter);

        ShowPinImage(tableUI.IsPinnedCharacter(runtimeCharacter));
        backgroundImage.SetActive(false);

        Refresh();
    }

    private void OnDestroy()
    {
        if (runtimeCharacter != null)
            runtimeCharacter.OnStatChanged -= Refresh;

        if (UnlockManager.Instance != null)
            UnlockManager.Instance.OnUnlockChanged -= Refresh;
    }

    public void Refresh()
    {
        if (runtimeCharacter == null)
            return;

        CharacterStatistics stat =
            StatisticsManager.Instance.GetCurrentStatistics(runtimeCharacter);

        nameText.text = runtimeCharacter.OriginCharacter.characterName;
        winRateText.text = $"{stat.Winrate:F1}%";
        pickRateText.text = $"{AnalysisManager.Instance.GetPickRate(runtimeCharacter):F1}%";

        tierText.text = UnlockManager.Instance.IsUnlocked(UnlockShowTier)
            ? $"{AnalysisManager.Instance.GetTier(runtimeCharacter)}"
            : " - ";

        banRateText.text = UnlockManager.Instance.IsUnlocked(UnlockShowBan)
            ? " - "
            : " - ";

        damageText.text = $"{stat.AverageDamage:F0}";

        float dps = stat.AverageSurvivalTime <= 0f
            ? 0f
            : stat.AverageDamage / stat.AverageSurvivalTime;

        DPSText.text = UnlockManager.Instance.IsUnlocked(UnlockShowDPS)
            ? $"{dps:F1}"
            : " - ";

        livetimeText.text = UnlockManager.Instance.IsUnlocked(UnlockShowLivetime)
            ? $"{AnalysisManager.Instance.GetAnalysis(runtimeCharacter, AnalysisItem.AverageLiveTime).CurrentValue:F1}"
            : " - ";
    }

    public void UpdateRank(int rank)
    {
        rankText.SetText(rank.ToString());
    }

    public void UpdateExpectedWinRate(float winRate)
    {
        if (winRateText == null)
            return;

        winRateText.text = $"({winRate:F1}%)";
    }

    //========================================================
    // Click
    //========================================================

    public void OnClickLeft()
    {
        if (runtimeCharacter == null)
            return;

        Debug.Log(runtimeCharacter.OriginCharacter.name);

        InspectorUI.Instance.Showcharacter(runtimeCharacter);
        BottomDisplayUI.Instance.SkillDescription.Initialize(runtimeCharacter.OriginCharacter, Symbols);
        UIManager.Instance.characterPreviewPopupUI.SetCharacter(runtimeCharacter.OriginCharacter);
    }

    public void OnClickRight()
    {
        if (runtimeCharacter == null)
            return;

        tableUI.TogglePinCharacter(this);
    }

    public void ShowPinImage(bool show)
    {
        PinImage.gameObject.SetActive(show);
    }

    //========================================================
    // Hover
    //========================================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.SetActive(false);
    }
}
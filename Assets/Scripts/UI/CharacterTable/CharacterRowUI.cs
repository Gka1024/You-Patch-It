using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
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

    private const int UnlockShowTier = 1021;
    private const int UnlockShowBan = 1022;
    private const int UnlockShowLivetime = 1023;
    private const int UnlockShowDPS = 1024;

    public RuntimeCharacter RuntimeCharacter => runtimeCharacter;

    public void Initialize(RuntimeCharacter character, CharacterTableUI tableUI)
    {
        runtimeCharacter = character;
        this.tableUI = tableUI;

        Symbols.sprite = tableUI.GetSymbolSprite(character);
        RowClickChecker.Initialize(this);
        Refresh();

        runtimeCharacter.OnStatChanged += Refresh;
        UnlockManager.Instance.OnUnlockChanged += Refresh;
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
        CharacterStatistics stat = StatisticsManager.Instance.GetCurrentStatistics(runtimeCharacter);

        nameText.text = runtimeCharacter.OriginCharacter.characterName;

        winRateText.text = $"{stat.Winrate:F1}%";

        pickRateText.text = $"{AnalysisManager.Instance.GetPickRate(runtimeCharacter):F1}%";

        tierText.text = UnlockManager.Instance.IsUnlocked(UnlockShowTier) ? $"{AnalysisManager.Instance.GetTier(runtimeCharacter)}" : " - ";

        banRateText.text = UnlockManager.Instance.IsUnlocked(UnlockShowBan) ? $" - " : $" - ";

        damageText.text = $"{stat.AverageDamage:F0}";

        float dps = stat.AverageSurvivalTime <= 0f ? 0f : stat.AverageDamage / stat.AverageSurvivalTime;
        DPSText.text = UnlockManager.Instance.IsUnlocked(UnlockShowDPS) ? $"{dps:F1}" : " - ";

        livetimeText.text = UnlockManager.Instance.IsUnlocked(UnlockShowLivetime) ?
            $"{AnalysisManager.Instance.GetAnalysis(runtimeCharacter, AnalysisItem.AverageLiveTime).CurrentValue:F1}" : " - ";
    }

    // =========================================================
    // Click
    // =========================================================

    public void OnClickLeft()
    {
        Debug.Log(runtimeCharacter.OriginCharacter.name);

        InspectorUI.Instance.Showcharacter(runtimeCharacter);
        BottomDisplayUI.Instance.SkillDescription.Initialize(runtimeCharacter.OriginCharacter, Symbols);
        UIManager.Instance.characterPreviewPopupUI.SetCharacter(runtimeCharacter.OriginCharacter);
    }

    public void OnClickRight()
    {
        tableUI.TogglePinCharacter(this);
    }

    public void ShowPinImage(bool show)
    {
        PinImage.gameObject.SetActive(show);
    }

    // =========================================================
    // Hover
    // =========================================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.SetActive(false);
    }
}
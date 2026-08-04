using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterRowUI : MonoBehaviour
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

    [SerializeField] private Button button;

    [SerializeField] private RuntimeCharacter runtimeCharacter;

    private const int UnlockShowTier = 1021;
    private const int UnlockShowBan = 1022;
    private const int UnlockShowLivetime = 1023;
    private const int UnlockShowDPS = 1024;

    public void Initialize(RuntimeCharacter character)
    {
        runtimeCharacter = character;

        Refresh();

        button.onClick.AddListener(OnClick);

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

        winRateText.text = $"{stat.WinRate:F1}%";

        pickRateText.text = $"{AnalysisManager.Instance.GetPickRate(runtimeCharacter):F1}%";

        tierText.text = UnlockManager.Instance.IsUnlocked(UnlockShowTier) ? $"{AnalysisManager.Instance.GetTier(runtimeCharacter)}" : " - "; // 티어 확인

        banRateText.text = UnlockManager.Instance.IsUnlocked(UnlockShowBan) ? $" - " : $" - "; // 밴 추가 후 수정

        damageText.text = $"{stat.AverageDamage:F0}";

        float dps = stat.AverageSurvivalTime <= 0f ? 0f : stat.AverageDamage / stat.AverageSurvivalTime;
        DPSText.text = UnlockManager.Instance.IsUnlocked(UnlockShowDPS) ? $"{dps:F1}" : " - ";

        livetimeText.text = UnlockManager.Instance.IsUnlocked(UnlockShowLivetime) ?
        $"{AnalysisManager.Instance.GetAnalysis(runtimeCharacter, AnalysisItem.AverageLiveTime).CurrentValue:F1}" : " - ";
    }

    private void OnClick()
    {
        Debug.Log(runtimeCharacter.OriginCharacter.name);
        InspectorUI.Instance.Show(runtimeCharacter);
        InspectorUI.Instance.ShowStats();
    }
}
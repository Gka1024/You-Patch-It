using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterRowUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text winRateText;
    [SerializeField] private TMP_Text pickRateText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text DPSText;
    [SerializeField] private Button button;

    [SerializeField] private RuntimeCharacter runtimeCharacter;

    public void Initialize(RuntimeCharacter character)
    {
        runtimeCharacter = character;

        Refresh();

        button.onClick.AddListener(OnClick);

        runtimeCharacter.OnStatChanged += Refresh;
    }

    private void OnDestroy()
    {
        if (runtimeCharacter != null)
            runtimeCharacter.OnStatChanged -= Refresh;
    }

    public void Refresh()
    {
        CharacterStatistics stat = StatisticsManager.Instance.GetCurrentStatistics(runtimeCharacter);

        nameText.text = runtimeCharacter.OriginCharacter.characterName;

        winRateText.text = $"{stat.WinRate:F1}%";

        pickRateText.text = $"{AnalysisManager.Instance.GetPickRate(runtimeCharacter)}%";

        damageText.text = $"{stat.AverageDamage:F0}";

        float dps = stat.AverageSurvivalTime <= 0f ? 0f : stat.AverageDamage / stat.AverageSurvivalTime;
        DPSText.text = $"{dps:F1}";
    }

    private void OnClick()
    {
        Debug.Log(runtimeCharacter.OriginCharacter.name);
        InspectorUI.Instance.Show(runtimeCharacter);
        //InspectorUI.Instance.ShowStats();
    }
}
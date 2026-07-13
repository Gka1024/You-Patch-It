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
    [SerializeField] private TMP_Text KDAText;
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
        CharacterStatistics stat = StatisticsManager.Instance.GetStatistics(runtimeCharacter);

        nameText.text = runtimeCharacter.OriginCharacter.characterName;

        winRateText.text = $"{stat.WinRate:F1}%";

        pickRateText.text = $"{StatisticsManager.Instance.GetPickRate(runtimeCharacter) * 100}%";

        damageText.text = $"{stat.AverageDamage:F0}";
    }

    private void OnClick()
    {
        Debug.Log(runtimeCharacter.OriginCharacter.name);
        InspectorUI.Instance.Show(runtimeCharacter);
        InspectorUI.Instance.ShowStats();
    }
}
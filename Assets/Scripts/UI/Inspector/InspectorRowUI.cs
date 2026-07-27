using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectorRowUI : MonoBehaviour
{
    [Header("Stat")]
    [SerializeField] private CharacterStatType statType;
    [SerializeField] private float changeUnit;

    [Header("UI")]
    [SerializeField] private TMP_Text statName;

    [SerializeField] private TMP_Text currentValue;
    [SerializeField] private float currentValueFloat;

    [SerializeField] private TMP_Text changeValue;
    [SerializeField] private float changeValueFloat;

    [SerializeField] private TMP_Text afterValue;

    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;


    void Awake()
    {
        minusButton.onClick.AddListener(MinusClick);
        plusButton.onClick.AddListener(PlusClick);
    }

    public void Initialize(RuntimeCharacter character)
    {
        currentValueFloat = character.GetStat(statType);
        currentValue.text = currentValueFloat.ToString("0.##");

        changeValueFloat = 0;
        RefreshUI();
    }

    private void RefreshUI()
    {
        changeValue.text = changeValueFloat.ToString("0.##");
        afterValue.text = (currentValueFloat + changeValueFloat).ToString("0.##");
    }

    private void MinusClick()
    {
        changeValueFloat -= changeUnit;
        RefreshUI();
    }

    private void PlusClick()
    {
        changeValueFloat += changeUnit;
        RefreshUI();
    }

    public CharacterPatch GetPatch()
    {
        return new CharacterPatch(statType, currentValueFloat, currentValueFloat + changeValueFloat);
    }

}

public readonly struct CharacterPatch
{
    public CharacterStatType StatType { get; }
    public float Before { get; }
    public float After { get; }

    public CharacterPatch(CharacterStatType statType, float before, float after)
    {
        StatType = statType;
        Before = before;
        After = after;
    }
}
using TMPro;
using UnityEngine;

public class InspectorRowUI : MonoBehaviour
{
    [Header("Stat")]
    [SerializeField] private CharacterStatType statType;
    public CharacterStatType StatType => statType;
    [SerializeField] private float changeUnit;

    [Header("UI")]
    [SerializeField] private TMP_Text statName;
    [SerializeField] private TMP_Text currentValue;
    [SerializeField] private float currentValueFloat;

    [SerializeField] private TMP_Text changeValue;
    [SerializeField] private float changeValueFloat;

    [SerializeField] private TMP_Text afterValue;

    [SerializeField] private InspectorLongPressButton minusButton;
    [SerializeField] private InspectorLongPressButton plusButton;

    private void Awake()
    {
        minusButton.onClick += MinusClick;
        plusButton.onClick += PlusClick;
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

        if (currentValueFloat + changeValueFloat <= 0)
        {
            changeValueFloat += changeUnit;
            return;
        }

        RefreshUI();
    }

    private void PlusClick()
    {
        changeValueFloat += changeUnit;
        RefreshUI();
    }

    public bool HasChange()
    {
        return changeValueFloat != 0;
    }

    public void ResetValue()
    {
        changeValueFloat = 0f;
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
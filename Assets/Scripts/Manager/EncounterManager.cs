using System.Collections.Generic;
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance;

    public EncounterList encounterList;

    private Dictionary<int, Encounter> encounterDictionaryNegative = new();

    [SerializeField] private EncounterPopupUI encounterPopup;

    private void Awake()
    {
        Instance = this;
        RegisterEncounter();
    }

    private void RegisterEncounter()
    {
        foreach (Encounter encounter in encounterList.NegativeEncounters)
        {
            encounterDictionaryNegative.Add(encounter.id, encounter);
        }
    }

    public void TestFunction()
    {
        ApplyEncounter(GetEncounter(1));
    }

    public void ApplyEncounter(Encounter encounter)
    {
        switch (encounter.goodsType)
        {
            case GoodsType.Trust:
                ApplyTrust(encounter);
                break;

            case GoodsType.DevelopResource:
                ApplyDevelopResource(encounter);
                break;

            case GoodsType.Player:
                ApplyPlayer(encounter);
                break;

            case GoodsType.Money:
                ApplyMoney(encounter);
                break;
        }

        encounterPopup.gameObject.SetActive(true);
        encounterPopup.Initialize(encounter);
    }

    public Encounter GetRandomEncounterNegative()
    {
        if (encounterDictionaryNegative.Count == 0)
            return null;

        List<Encounter> encounters = new(encounterDictionaryNegative.Values);

        int index = Random.Range(0, encounters.Count);

        return encounters[index];
    }

    public Encounter GetEncounter(int id)
    {
        encounterDictionaryNegative.TryGetValue(id, out Encounter encounter);
        return encounter;
    }

    private void ApplyTrust(Encounter encounter)
    {
        float value = CalculateValue(ResourceManager.Instance.GetTrust, encounter);
        ResourceManager.Instance.AddTrust(value);
    }

    private void ApplyDevelopResource(Encounter encounter)
    {
        int value = Mathf.RoundToInt(CalculateValue(ResourceManager.Instance.GetDevelop, encounter));
        ResourceManager.Instance.AddDevelopResource(value);
    }

    private void ApplyMoney(Encounter encounter)
    {
        int value = Mathf.RoundToInt(CalculateValue(ResourceManager.Instance.GetMoney, encounter));

        if (value >= 0)
            ResourceManager.Instance.AddMoney(value);
        else
            ResourceManager.Instance.SpendMoney(-value);
    }

    private void ApplyPlayer(Encounter encounter)
    {
        if (encounter.valueType2 == ValueType2.Current)
        {
            int currentPlayer = PlayerManager.Instance.CurrentPlayerCount;

            int value = encounter.valueType == ValueType.Percent
                ? Mathf.RoundToInt(currentPlayer * encounter.value * 0.01f)
                : Mathf.RoundToInt(encounter.value);

            PlayerManager.Instance.AddPlayerCount(value);
            return;
        }

        PlayerManager.Instance.AddSeasonPlayerModifier(encounter);
    }

    private float CalculateValue(float currentValue, Encounter encounter)
    {
        if (encounter.valueType == ValueType.Flat)
            return encounter.value;

        return currentValue * encounter.value * 0.01f;
    }
}

public enum GoodsType
{
    Trust,
    DevelopResource,
    Player,
    Money
}

public enum ValueType
{
    Percent,
    Flat
}

public enum ValueType2
{
    Current,
    Season
}
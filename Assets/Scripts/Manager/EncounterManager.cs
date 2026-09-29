
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
        encounterDictionaryNegative.Clear();

        foreach (Encounter encounter in encounterList.NegativeEncounters)
        {
            encounterDictionaryNegative.Add(encounter.id, encounter);
        }
    }

    public void TestFunction()
    {
        ApplyEncounter(GetEncounter(1));
    }

    // =========================================================
    // Apply Encounter
    // =========================================================

    public void ApplyEncounter(Encounter encounter)
    {
        if (encounter == null)
            return;

        foreach (EncounterResource resource in encounter.EncounterResources)
        {
            ApplyResource(resource);
        }

        encounterPopup.gameObject.SetActive(true);
        encounterPopup.Initialize(encounter);
    }

    private void ApplyResource(EncounterResource resource)
    {
        switch (resource.goodsType)
        {
            case GoodsType.Trust:
                ApplyTrust(resource);
                break;

            case GoodsType.DevelopResource:
                ApplyDevelopResource(resource);
                break;

            case GoodsType.Player:
                ApplyPlayer(resource);
                break;

            case GoodsType.Money:
                ApplyMoney(resource);
                break;
        }
    }

    // =========================================================
    // Trust
    // =========================================================

    private void ApplyTrust(EncounterResource resource)
    {
        float value = CalculateValue(ResourceManager.Instance.GetTrust, resource);

        ResourceManager.Instance.AddTrust(value);
    }

    // =========================================================
    // Develop Resource
    // =========================================================

    private void ApplyDevelopResource(EncounterResource resource)
    {
        int value = Mathf.RoundToInt(CalculateValue(ResourceManager.Instance.GetDevelop, resource));

        ResourceManager.Instance.AddDevelopResource(value);

    }

    // =========================================================
    // Money
    // =========================================================

    private void ApplyMoney(EncounterResource resource)
    {
        int value = Mathf.RoundToInt(CalculateValue(ResourceManager.Instance.GetMoney, resource));

        if (value >= 0)
        {
            ResourceManager.Instance.AddMoney(value);
        }
        else
        {
            ResourceManager.Instance.SpendMoney(-value);
        }
    }

    // =========================================================
    // Player
    // =========================================================

    private void ApplyPlayer(EncounterResource resource)
    {
        if (resource.valuePeriod == ValuePeriod.Total)
        {
            int currentPlayer = PlayerManager.Instance.CurrentPlayerCount;

            int value = Mathf.RoundToInt(CalculateValue(currentPlayer, resource));

            PlayerManager.Instance.AddPlayerCount(value);
        }
        else
        {
            PlayerManager.Instance.AddSeasonPlayerModifier(resource);
        }
    }

    // =========================================================
    // Calculate
    // =========================================================

    private float CalculateValue(float currentValue, EncounterResource resource)
    {
        float value;

        if (resource.valueFormat == ValueFormat.Flat)
        {
            value = resource.value;
        }
        else
        {
            value = currentValue * resource.value * 0.01f;
        }

        if (resource.isNegative)
        {
            value = -value;
        }

        return value;
    }

    // =========================================================
    // Get Encounter
    // =========================================================

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
}

public enum GoodsType
{
    Trust,
    DevelopResource,
    Player,
    Money
}

public enum ValueFormat
{
    Percent,
    Flat
}

public enum ValuePeriod
{
    Total,
    CurrentSeason
}
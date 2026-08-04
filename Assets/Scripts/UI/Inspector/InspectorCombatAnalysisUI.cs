using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InspectorCombatAnalysisUI : MonoBehaviour
{
    private List<RuntimeCharacter> characters;

    [SerializeField] private TMP_Text baseCharacterText;

    [SerializeField] private TMP_Dropdown opponentDropdown;
    [SerializeField] private TMP_Dropdown tierDropdown;

    private RuntimeCharacter baseCharacter;
    private RuntimeCharacter opponentCharacter;
    private PlayerTier? selectedTier;

    [Header("UI")]
    [SerializeField] private TMP_Text currentWinrate;
    [SerializeField] private TMP_Text currentMatchCount;

    [SerializeField] private TMP_Text pastWinrate;
    [SerializeField] private TMP_Text pastMatchCount;

    public void Initialize(RuntimeCharacter character)
    {
        baseCharacter = character;
        baseCharacterText.text = character.OriginCharacter.characterName;

        characters = RuntimeCharacterManager.Instance.GetAllCharacters().ToList();
        //characters = RuntimeCharacterManager.Instance.GetAllCharacters().Where(x => x != character).ToList();

        RegisterOpponentDropdown();
        RegisterTierDropdown();

        opponentCharacter = null;
        selectedTier = null;

        Refresh();
    }

    private void RegisterOpponentDropdown()
    {
        opponentDropdown.onValueChanged.RemoveAllListeners();

        List<string> options = new() { "전체" };

        options.AddRange(characters.Select(x => x.OriginCharacter.characterName));

        opponentDropdown.ClearOptions();
        opponentDropdown.AddOptions(options);

        opponentDropdown.value = 0;
        opponentDropdown.onValueChanged.AddListener(OnOpponentChanged);
    }

    private void RegisterTierDropdown()
    {
        tierDropdown.onValueChanged.RemoveAllListeners();

        List<string> options = new() { "전체" };

        options.AddRange(Enum.GetNames(typeof(PlayerTier)));

        tierDropdown.ClearOptions();
        tierDropdown.AddOptions(options);

        tierDropdown.value = 0;
        tierDropdown.onValueChanged.AddListener(OnTierChanged);
    }

    private void OnOpponentChanged(int index)
    {
        if (index == 0)
            opponentCharacter = null;
        else
            opponentCharacter = characters[index - 1];

        Refresh();
    }

    private void OnTierChanged(int index)
    {
        if (index == 0)
            selectedTier = null;
        else
            selectedTier = (PlayerTier)(index - 1);

        Refresh();
    }

    private void Refresh()
    {
        MatchUpData data = AnalysisManager.Instance.GetMatchupData(baseCharacter, opponentCharacter, selectedTier);
        SetText(data);
    }

    private void SetText(MatchUpData data)
    {
        currentWinrate.text = $"{data.CurrentWinRate:F1}%";
        currentMatchCount.text = data.CurrentMatchCount.ToString();

        if (data.HasPastData)
        {
            pastWinrate.text = $"{data.PastWinRate:F1}%";
            pastMatchCount.text = data.PastMatchCount.ToString();
        }
        else
        {
            pastWinrate.text = "-";
            pastMatchCount.text = "-";
        }
    }
}

public class MatchUpData
{
    public bool HasPastData;

    public float CurrentWinRate;
    public int CurrentMatchCount;

    public float PastWinRate;
    public int PastMatchCount;
}
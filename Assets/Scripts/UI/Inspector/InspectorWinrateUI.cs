using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InspectorWinrateUI : MonoBehaviour
{
    private List<RuntimeCharacter> characters;

    [SerializeField] private TMP_Text BaseCharacterText;
    [SerializeField] private TMP_Dropdown OpponentDropdown;

    private RuntimeCharacter baseCharacter;
    private RuntimeCharacter opponentCharacter;

    [Header("UI")]
    [SerializeField] private TMP_Text currentWinrate;
    [SerializeField] private TMP_Text currentMatchCount;

    [SerializeField] private TMP_Text pastWinrate;
    [SerializeField] private TMP_Text pastMatchCount;

    [SerializeField] private TMP_Text deltaWinrate;
    [SerializeField] private TMP_Text deltaMatchCount;

    public void Initialize(RuntimeCharacter character)
    {
        characters = RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        List<string> names = characters.Select(character => character.OriginCharacter.characterName).ToList();

        BaseCharacterText.text = $"{character.OriginCharacter.characterName}";
        baseCharacter = character;

        OpponentDropdown.ClearOptions();
        OpponentDropdown.AddOptions(names);
        OpponentDropdown.onValueChanged.AddListener(OnOpponentChanged);
    }

    private void OnOpponentChanged(int index)
    {
        opponentCharacter = characters[index];
        Refresh();
    }

    private void Refresh()
    {
        MatchUpData data = AnalysisManager.Instance.GetMatchupData(baseCharacter, opponentCharacter);

        SetText(data);
    }

    private void SetText(MatchUpData data)
    {
        currentWinrate.text = $"{data.CurrentWinRate:0.0}%";
        currentMatchCount.text = $"{data.CurrentMatchCount}";

        pastWinrate.text = $"{data.PastWinRate:0.0}%";
        pastMatchCount.text = $"{data.PastMatchCount}";

        float deltaWinrateValue = data.PastWinRate - data.CurrentWinRate;
        int deltaMatchCountValue = data.PastMatchCount - data.CurrentMatchCount;

        deltaWinrate.text = deltaWinrateValue > 0 ? $"▲{deltaWinrateValue:0.0}%" : $"▼{-deltaWinrateValue:0.0}%";
        deltaMatchCount.text = deltaMatchCountValue > 0 ? $"▲{deltaMatchCountValue}%" : $"▼{-deltaMatchCountValue}%";
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
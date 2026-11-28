using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonReportRowUI : MonoBehaviour
{
    private RuntimeCharacter character;

    [SerializeField] private TMP_Text CharacterName;
    [SerializeField] private TMP_Text WinrateText;
    [SerializeField] private TMP_Text PickrateText;
    [SerializeField] private Button GotoPatchNoteButton;

    private void Awake()
    {
        GotoPatchNoteButton.onClick.AddListener(MoveTo);
    }

    public void Initialize(RuntimeCharacter character, List<CharacterStatistics> stats, int currentSeason)
    {
        this.character = character;

        SetText(character, stats, currentSeason);
    }

    private void SetText(RuntimeCharacter character, List<CharacterStatistics> stats, int currentSeason)
    {
        CharacterName.text = $"{character.OriginCharacter.characterName} : ";

        string winrateTextToWrite = "";
        string pickrateTextToWrite = "";

        bool isNewCharacter =
            RuntimeCharacterManager.Instance.AddedRuntimeCharacter != null &&
            RuntimeCharacterManager.Instance.AddedRuntimeCharacter.OriginCharacter.id ==
            character.OriginCharacter.id;

        if (isNewCharacter)
        {
            winrateTextToWrite = "신규 추가됨";
            pickrateTextToWrite = " - ";
        }
        else
        {
            winrateTextToWrite = string.Join(" - ", stats.ConvertAll(stat => $"{stat.Winrate:F1}"));

            float averagePickRate = AnalysisManager.Instance.GetSeasonPickRate(character.OriginCharacter.id, currentSeason);

            pickrateTextToWrite = $"{averagePickRate:F1}%";
        }

        WinrateText.text = winrateTextToWrite;
        PickrateText.text = pickrateTextToWrite;
    }

    private void MoveTo()
    {
        UIManager.Instance.patchNoteUI.Start();
        UIManager.Instance.patchNoteUI.ShowCharacterOnSeasonReport(character);
        UIManager.Instance.dashBoardUI.ShowPatchNote();
    }
}
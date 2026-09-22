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

    void Awake()
    {
        GotoPatchNoteButton.onClick.AddListener(MoveTo);
    }

    public void Initialize(RuntimeCharacter character, List<CharacterStatistics> stats)
    {
        this.character = character;
        SetText(character, stats);
    }

    private void SetText(RuntimeCharacter character, List<CharacterStatistics> stats)
    {
        CharacterName.text = character.OriginCharacter.characterName;

        string WinrateTextToWrite = "";
        string PickrateTextToWrite = "";

        bool isNewCharacter =
            RuntimeCharacterManager.Instance.AddedRuntimeCharacter != null &&
            RuntimeCharacterManager.Instance.AddedRuntimeCharacter.OriginCharacter.id ==
            character.OriginCharacter.id;

        if (isNewCharacter)
        {
            WinrateTextToWrite += "신규 추가됨";
            PickrateTextToWrite += " - ";
        }
        else
        {
            WinrateTextToWrite += string.Join(" - ", stats.ConvertAll(stat => $"{stat.Winrate:F1}"));
            PickrateTextToWrite += $"{AnalysisManager.Instance.GetPickRate(character):F1}%";
        }

        WinrateText.text = WinrateTextToWrite;
        PickrateText.text = PickrateTextToWrite;
    }

    private void MoveTo()
    {
        UIManager.Instance.patchNoteUI.Start();
        UIManager.Instance.patchNoteUI.ShowCharacterOnSeasonReport(character);
        UIManager.Instance.dashBoardUI.ShowPatchNote();
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonReportRowUI : MonoBehaviour
{
    [SerializeField] private RuntimeCharacter character;
    [SerializeField] private TMP_Text RowText;
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
        string textToWrite = "";
        textToWrite += $"{character.OriginCharacter.characterName} : ";

        if (RuntimeCharacterManager.Instance.AddedRuntimeCharacter.OriginCharacter.id == character.OriginCharacter.id)
        {
            textToWrite += "신규 추가됨";
        }
        else
        {
            for (int i = 0; i < stats.Count; i++)
            {
                textToWrite += $" {stats[i].Winrate:F1}";

                if (i != stats.Count - 1)
                {
                    textToWrite += " - ";
                }
            }
        }

        RowText.text = textToWrite;
    }

    private void MoveTo()
    {
        UIManager.Instance.patchNoteUI.Start();
        UIManager.Instance.patchNoteUI.ShowCharacterOnSeasonReport(character);
        UIManager.Instance.dashBoardUI.ShowPatchNote();
    }
}
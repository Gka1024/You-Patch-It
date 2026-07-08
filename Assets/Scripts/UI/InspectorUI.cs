using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectorUI : MonoBehaviour
{
    public static InspectorUI Instance;

    private RuntimeCharacter currentCharacter;

    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button applyPatchButton;

    [Header("Rows")]
    [SerializeField] private GameObject[] rows;

    private void Awake()
    {
        Instance = this;
        applyPatchButton.onClick.AddListener(ApplyPatch);
    }

    public void Show(RuntimeCharacter character)
    {
        currentCharacter = character;
        ShowRows();
        Refresh();
    }

    public void Refresh()
    {
        if (currentCharacter == null)
        {
            return;
        }

        nameText.text = currentCharacter.OriginCharacter.characterName;
    }

    private void ShowRows()
    {
        foreach (var row in rows)
        {
            row.SetActive(true);
            row.GetComponent<InspectorRowUI>().Initialize(currentCharacter);
        }
    }

    private void ApplyPatch()
    {
        if (currentCharacter == null)
            return;

        List<CharacterPatch> patches = new();

        foreach (GameObject row in rows)
        {
            InspectorRowUI rowUI = row.GetComponent<InspectorRowUI>();
            patches.Add(rowUI.GetPatch());
        }

        currentCharacter.Patch(patches);

        Refresh();
        ShowRows();
    }
}
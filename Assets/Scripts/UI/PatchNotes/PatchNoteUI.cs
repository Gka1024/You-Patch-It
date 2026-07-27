using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PatchNoteUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown characterDropdown;

    [SerializeField] private Transform content;

    [SerializeField] private PatchNoteItemUI patchNotePrefab;

    private RuntimeCharacter currentCharacter;

    private void Awake()
    {
        characterDropdown.onValueChanged.AddListener(OnCharacterChanged);
    }

    private void Start()
    {
        InitializeDropdown();
    }

    public void InitializeDropdown()
    {
        characterDropdown.ClearOptions();

        List<RuntimeCharacter> characters =
            RuntimeCharacterManager.Instance
            .GetAllCharacters()
            .OrderBy(x => x.OriginCharacter.id)
            .ToList();

        characterDropdown.AddOptions(
            characters.Select(x => x.OriginCharacter.characterName).ToList());

        if (characters.Count > 0)
            ShowCharacter(characters[0]);
    }

    private void OnCharacterChanged(int index)
    {
        RuntimeCharacter character =
            RuntimeCharacterManager.Instance
            .GetAllCharacters()
            .OrderBy(x => x.OriginCharacter.id)
            .ElementAt(index);

        ShowCharacter(character);
    }

    private void ShowCharacter(RuntimeCharacter character)
    {
        currentCharacter = character;

        foreach (Transform child in content)
            Destroy(child.gameObject);

        List<PatchHistory> histories = PatchHistoryManager.Instance.GetHistories(character).ToList();

        foreach (PatchHistory history in histories)
        {
            PatchNoteItemUI item =
                Instantiate(patchNotePrefab, content);

            item.Initialize(history);
        }
    }
}

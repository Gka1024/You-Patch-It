using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PatchNoteUI : MonoBehaviour
{
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Dropdown characterDropdown;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject patchNotePrefab;
    private readonly List<PatchNoteItemUI> items = new();

    [SerializeField] private GameObject noPatchAlert;

    private RuntimeCharacter currentCharacter;

    private void Awake()
    {
        characterDropdown.onValueChanged.AddListener(OnCharacterChanged);
    }

    private void Start()
    {
        InitializeDropdown();
        currentCharacter = RuntimeCharacterManager.Instance.GetRuntimeCharacter(101);
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
        characterName.text = character.OriginCharacter.characterName;
        foreach (Transform child in content)
            Destroy(child.gameObject);
        items.Clear();

        List<PatchHistory> histories = PatchHistoryManager.Instance.GetHistories(character).ToList();

        noPatchAlert.SetActive(histories.Count == 0);

        foreach (PatchHistory history in histories)
        {
            PatchNoteItemUI item = Instantiate(patchNotePrefab, content).GetComponent<PatchNoteItemUI>();

            item.Initialize(history);

            item.OnHeightChanged += Rearrange;
            item.OnClicked += OnItemClicked;

            items.Add(item);
            Rearrange();
        }
    }

    public void Refresh()
    {
        if (currentCharacter == null)
        {
            currentCharacter = RuntimeCharacterManager.Instance.GetRuntimeCharacter(101);
        }

        ShowCharacter(currentCharacter);
    }

    public void CloseAll()
    {
        foreach (PatchNoteItemUI item in items)
        {
            item.SetOpen(false, true);
        }
    }

    private void OnItemClicked(PatchNoteItemUI clickedItem)
    {
        foreach (PatchNoteItemUI item in items)
        {
            if (item == clickedItem)
            {
                continue;
            }

            if (item.IsOpened)
            {
                item.SetOpen(false);
            }
        }

        clickedItem.SetOpen(!clickedItem.IsOpened);
    }

    private void Rearrange(PatchNoteItemUI changed = null)
    {
        float y = 0;

        foreach (PatchNoteItemUI item in items)
        {
            item.Rect.anchoredPosition =
                new Vector2(
                    item.Rect.anchoredPosition.x,
                    -y);

            y += item.CurrentHeight;
        }

        RectTransform rect = content as RectTransform;

        rect.sizeDelta =
            new Vector2(
                rect.sizeDelta.x,
                y);
    }
}

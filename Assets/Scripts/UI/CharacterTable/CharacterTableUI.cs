
using System.Collections.Generic;
using UnityEngine;

public class CharacterTableUI : MonoBehaviour
{
    [SerializeField] private CharacterRowUI rowPrefab;
    [SerializeField] private Transform content;

    [SerializeField] private AnalysisItem currentSortItem;
    [SerializeField] private SortDirection currentDirection = SortDirection.Descending;

    [SerializeField] private List<CharacterTableHeaderUI> headers = new();

    [SerializeField] private Sprite warriorSprite;
    [SerializeField] private Sprite rangedSprite;
    [SerializeField] private Sprite mageSprite;
    [SerializeField] private Sprite assassinSprite;
    [SerializeField] private Sprite tankSprite;
    [SerializeField] private Sprite supportSprite;

    private readonly List<CharacterRowUI> rowList = new();

    private Dictionary<RuntimeCharacter, CharacterRowUI> rowMap = new();

    [SerializeField] private List<GameObject> rankNumList;

    private CharacterRowUI pinnedCharacter;
    private Character pinnedOriginCharacter;

    private void Start()
    {
        GenerateTable();
        InitializeHeaders();
    }

    // =========================================================
    // Generate
    // =========================================================

    public void GenerateTable()
    {
        ClearTable();

        foreach (RuntimeCharacter runtimeCharacter in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            CharacterRowUI row = Instantiate(rowPrefab, content);

            row.Initialize(runtimeCharacter, this);

            rowList.Add(row);
            rowMap.Add(runtimeCharacter, row);
        }

        DisplayRankNumber(rowList.Count);

        RestorePinnedCharacter();

        ArrangeTable(currentSortItem, currentDirection);
    }

    private void InitializeHeaders()
    {
        foreach (CharacterTableHeaderUI header in headers)
        {
            header.Initialize(this);
        }
    }

    // =========================================================
    // Refresh
    // =========================================================

    public void RefreshTable()
    {
        foreach (CharacterRowUI row in rowList)
        {
            row.Refresh();
        }

        DisplayRankNumber(rowList.Count);
    }

    public void ReArrangetable()
    {
        ArrangeTable(currentSortItem, currentDirection);
    }

    // =========================================================
    // Pin
    // =========================================================

    public void TogglePinCharacter(CharacterRowUI row)
    {
        if (row == null)
            return;

        if (pinnedCharacter == row)
        {
            pinnedCharacter.ShowPinImage(false);

            pinnedCharacter = null;
            pinnedOriginCharacter = null;
        }
        else
        {
            if (pinnedCharacter != null)
            {
                pinnedCharacter.ShowPinImage(false);
            }

            pinnedCharacter = row;
            pinnedOriginCharacter = row.RuntimeCharacter.OriginCharacter;

            pinnedCharacter.ShowPinImage(true);
        }

        ArrangeTable(currentSortItem, currentDirection);
    }

    private void RestorePinnedCharacter()
    {
        pinnedCharacter = null;

        if (pinnedOriginCharacter == null)
            return;

        foreach (CharacterRowUI row in rowList)
        {
            if (row.RuntimeCharacter.OriginCharacter == pinnedOriginCharacter)
            {
                pinnedCharacter = row;
                pinnedCharacter.ShowPinImage(true);
                return;
            }
        }
    }

    // =========================================================
    // Arrange
    // =========================================================

    private void ArrangeTable(AnalysisItem item, SortDirection direction)
    {
        List<RuntimeCharacter> characters = AnalysisManager.Instance.GetSortedCharacters(item, direction);

        int siblingIndex = 0;

        if (pinnedCharacter != null)
        {
            if (rowList.Contains(pinnedCharacter))
            {
                pinnedCharacter.ShowPinImage(true);
                pinnedCharacter.transform.SetSiblingIndex(siblingIndex++);
            }
            else
            {
                pinnedCharacter.ShowPinImage(false);
                pinnedCharacter = null;
            }
        }

        for (int i = 0; i < characters.Count; i++)
        {
            RuntimeCharacter character = characters[i];

            if (!rowMap.TryGetValue(character, out CharacterRowUI row))
                continue;

            if (row == pinnedCharacter)
                continue;

            row.ShowPinImage(false);
            row.transform.SetSiblingIndex(siblingIndex++);
        }

        RefreshTable();
    }

    // =========================================================
    // Add Character
    // =========================================================

    public void AddCharacter(RuntimeCharacter runtimeCharacter)
    {
        if (rowMap.ContainsKey(runtimeCharacter))
            return;

        CharacterRowUI row = Instantiate(rowPrefab, content);

        row.Initialize(runtimeCharacter, this);

        rowList.Add(row);
        rowMap.Add(runtimeCharacter, row);

        DisplayRankNumber(rowList.Count);

        // 새 캐릭터가 기존에 고정된 캐릭터라면
        // 해당 행을 다시 연결
        if (pinnedOriginCharacter != null &&
            runtimeCharacter.OriginCharacter == pinnedOriginCharacter)
        {
            pinnedCharacter = row;
            pinnedCharacter.ShowPinImage(true);
        }

        ArrangeTable(currentSortItem, currentDirection);
    }

    // =========================================================
    // Clear
    // =========================================================

    private void ClearTable()
    {
        foreach (CharacterRowUI row in rowList)
        {
            if (row != null)
            {
                Destroy(row.gameObject);
            }
        }

        rowList.Clear();
        rowMap.Clear();

        pinnedCharacter = null;
    }

    // =========================================================
    // Rank
    // =========================================================

    private void DisplayRankNumber(int count)
    {
        for (int i = 0; i < 10; i++)
        {
            rankNumList[i].SetActive(count - 1 >= i);
        }
    }

    // =========================================================
    // Symbol
    // =========================================================

    public Sprite GetSymbolSprite(RuntimeCharacter character)
    {
        if (character == null)
        {
            return null;
        }

        return character.OriginCharacter.role switch
        {
            CharacterRole.Warrior => warriorSprite,
            CharacterRole.Ranged => rangedSprite,
            CharacterRole.Mage => mageSprite,
            CharacterRole.Assassin => assassinSprite,
            CharacterRole.Tank => tankSprite,
            CharacterRole.Support => supportSprite,
            _ => warriorSprite
        };
    }

    // =========================================================
    // Header
    // =========================================================

    public void OnClickHeader(AnalysisItem item)
    {
        if (currentSortItem == item)
        {
            currentDirection =
                currentDirection == SortDirection.Ascending
                ? SortDirection.Descending
                : SortDirection.Ascending;
        }
        else
        {
            currentSortItem = item;
            currentDirection = SortDirection.Descending;
        }

        ArrangeTable(currentSortItem, currentDirection);

        foreach (CharacterTableHeaderUI header in headers)
        {
            header.Refresh(currentSortItem == header.Item, currentDirection);
        }
    }
}
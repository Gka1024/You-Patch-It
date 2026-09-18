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

    private void Start()
    {
        GenerateTable();
        InitializeHeaders();
    }

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
    }

    private void InitializeHeaders()
    {
        foreach (CharacterTableHeaderUI header in headers)
        {
            header.Initialize(this);
        }
    }

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

    private void ArrangeTable(AnalysisItem item, SortDirection direction)
    {
        List<RuntimeCharacter> characters = AnalysisManager.Instance.GetSortedCharacters(item, direction);

        for (int i = characters.Count - 1; i >= 0; i--)
        {
            rowMap[characters[i]].transform.SetSiblingIndex(i);
        }

        RefreshTable();
    }

    public void AddCharacter(RuntimeCharacter runtimeCharacter)
    {
        CharacterRowUI row = Instantiate(rowPrefab, content);

        row.Initialize(runtimeCharacter, this);

        rowList.Add(row);
    }

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
    }

    private void DisplayRankNumber(int count)
    {
        for (int i = 0; i < 10; i++)
        {
            rankNumList[i].SetActive(count - 1 >= i);
        }
    }

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
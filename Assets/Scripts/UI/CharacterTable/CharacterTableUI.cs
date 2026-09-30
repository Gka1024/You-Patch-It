using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTableUI : MonoBehaviour
{
    private const int CHARACTERS_PER_PAGE = 10;

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

    [SerializeField] private List<GameObject> rankNumList;

    [Header("Page")]
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;

    private readonly List<CharacterRowUI> rowList = new();
    private readonly Dictionary<RuntimeCharacter, CharacterRowUI> rowMap = new();

    // 실제 화면에 표시되는 순서
    private readonly List<RuntimeCharacter> displayCharacters = new();

    private CharacterRowUI pinnedCharacter;
    private Character pinnedOriginCharacter;

    private int currentPage = 0;

    private int TotalPage =>
        Mathf.Max(1, Mathf.CeilToInt((float)displayCharacters.Count / CHARACTERS_PER_PAGE));

    private void Start()
    {
        GenerateTable();
        InitializeHeaders();

        previousPageButton.onClick.AddListener(PreviousPage);
        nextPageButton.onClick.AddListener(NextPage);
    }

    private void OnDestroy()
    {
        if (previousPageButton != null)
            previousPageButton.onClick.RemoveListener(PreviousPage);

        if (nextPageButton != null)
            nextPageButton.onClick.RemoveListener(NextPage);
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

        currentPage = 0;

        DisplayRankNumber(rowList.Count);

        RestorePinnedCharacter();

        ArrangeTable(currentSortItem, currentDirection);
    }

    private void InitializeHeaders()
    {
        foreach (CharacterTableHeaderUI header in headers)
            header.Initialize(this);
    }

    // =========================================================
    // Refresh
    // =========================================================

    public void RefreshTable()
    {
        foreach (CharacterRowUI row in rowList)
        {
            if (row != null)
                row.Refresh();
        }

        DisplayRankNumber(rowList.Count);

        RefreshCharacterPage();
    }

    public void ReArrangetable()
    {
        ArrangeTable(currentSortItem, currentDirection);
    }

    // =========================================================
    // Page
    // =========================================================

    private void RefreshCharacterPage()
    {
        if (displayCharacters.Count == 0)
        {
            currentPage = 0;
            UpdatePageButtons();
            return;
        }

        if (currentPage >= TotalPage)
            currentPage = TotalPage - 1;

        int startIndex = currentPage * CHARACTERS_PER_PAGE;
        int endIndex = Mathf.Min(
            startIndex + CHARACTERS_PER_PAGE,
            displayCharacters.Count);

        // 일단 모든 Row를 비활성화
        foreach (CharacterRowUI row in rowList)
        {
            if (row != null)
                row.gameObject.SetActive(false);
        }

        // 현재 페이지에 해당하는 Row만 활성화
        for (int i = startIndex; i < endIndex; i++)
        {
            RuntimeCharacter character = displayCharacters[i];

            if (rowMap.TryGetValue(character, out CharacterRowUI row))
                row.gameObject.SetActive(true);
        }

        UpdatePageButtons();
    }

    private void UpdatePageButtons()
    {
        if (previousPageButton != null)
            previousPageButton.interactable = currentPage > 0;

        if (nextPageButton != null)
            nextPageButton.interactable = currentPage < TotalPage - 1;
    }

    private void PreviousPage()
    {
        if (currentPage <= 0)
            return;

        currentPage--;

        RefreshCharacterPage();
    }

    private void NextPage()
    {
        if (currentPage >= TotalPage - 1)
            return;

        currentPage++;

        RefreshCharacterPage();
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
            // 고정 해제
            pinnedCharacter.ShowPinImage(false);

            pinnedCharacter = null;
            pinnedOriginCharacter = null;
        }
        else
        {
            // 기존 고정 해제
            if (pinnedCharacter != null)
                pinnedCharacter.ShowPinImage(false);

            // 새로운 캐릭터 고정
            pinnedCharacter = row;
            pinnedOriginCharacter = row.RuntimeCharacter.OriginCharacter;

            pinnedCharacter.ShowPinImage(true);
        }

        // 고정 상태가 바뀌면 1페이지로 이동
        currentPage = 0;

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
        List<RuntimeCharacter> sortedCharacters =
            AnalysisManager.Instance.GetSortedCharacters(item, direction);

        displayCharacters.Clear();

        // =====================================================
        // 1. 고정 캐릭터를 가장 먼저 추가
        // =====================================================

        if (pinnedCharacter != null &&
            rowMap.ContainsKey(pinnedCharacter.RuntimeCharacter))
        {
            displayCharacters.Add(pinnedCharacter.RuntimeCharacter);
        }

        // =====================================================
        // 2. 나머지 캐릭터를 정렬 순서대로 추가
        // =====================================================

        foreach (RuntimeCharacter character in sortedCharacters)
        {
            if (pinnedCharacter != null &&
                character == pinnedCharacter.RuntimeCharacter)
            {
                continue;
            }

            displayCharacters.Add(character);
        }

        // =====================================================
        // 3. Hierarchy도 displayCharacters 순서대로 정렬
        // =====================================================

        for (int i = 0; i < displayCharacters.Count; i++)
        {
            RuntimeCharacter character = displayCharacters[i];

            if (!rowMap.TryGetValue(character, out CharacterRowUI row))
                continue;

            row.ShowPinImage(row == pinnedCharacter);

            row.transform.SetSiblingIndex(i);
        }

        // =====================================================
        // 4. 현재 페이지 갱신
        // =====================================================

        RefreshTable();
    }

    // =========================================================
    // Add Character
    // =========================================================

    public void AddCharacter(RuntimeCharacter runtimeCharacter)
    {
        if (runtimeCharacter == null)
            return;

        if (rowMap.ContainsKey(runtimeCharacter))
            return;

        CharacterRowUI row = Instantiate(rowPrefab, content);

        row.Initialize(runtimeCharacter, this);

        rowList.Add(row);
        rowMap.Add(runtimeCharacter, row);

        DisplayRankNumber(rowList.Count);

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
                Destroy(row.gameObject);
        }

        rowList.Clear();
        rowMap.Clear();
        displayCharacters.Clear();

        pinnedCharacter = null;
    }

    // =========================================================
    // Rank
    // =========================================================

    private void DisplayRankNumber(int count)
    {
        for (int i = 0; i < rankNumList.Count; i++)
            rankNumList[i].SetActive(count - 1 >= i);
    }

    // =========================================================
    // Symbol
    // =========================================================

    public Sprite GetSymbolSprite(RuntimeCharacter character)
    {
        if (character == null)
            return null;

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

        // 정렬 기준이 바뀌면 1페이지부터
        currentPage = 0;

        ArrangeTable(currentSortItem, currentDirection);

        foreach (CharacterTableHeaderUI header in headers)
        {
            header.Refresh(
                currentSortItem == header.Item,
                currentDirection);
        }
    }
}
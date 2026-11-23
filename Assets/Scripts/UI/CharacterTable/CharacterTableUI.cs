using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTableUI : MonoBehaviour
{
    private const int CHARACTERS_PER_PAGE = 10;
    private const float SCROLL_INPUT_INTERVAL = 0.05f;

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

    // 실제 생성한 행. 최대 CHARACTERS_PER_PAGE개만 유지한다.
    private readonly List<CharacterRowUI> rowList = new();

    // 현재 화면에 표시되는 순서
    private readonly List<RuntimeCharacter> displayCharacters = new();

    // 캐릭터의 정렬 순위. 화면 표시 순서와 별도로 유지한다.
    private readonly Dictionary<RuntimeCharacter, int> rankMap = new();

    private Character pinnedOriginCharacter;

    // 현재 화면의 첫 번째 캐릭터 인덱스
    private int currentStartIndex;

    // 캐릭터가 새로 추가될 때, 예상 승률을 캐싱해두는 딕셔너리
    private readonly Dictionary<RuntimeCharacter, float> expectedWinRateCache = new();
    private bool showExpectWinrate = false;
    private RuntimeCharacter newcharacter;

    private float previousScrollTime;

    private int MaxStartIndex =>
        Mathf.Max(0, displayCharacters.Count - CHARACTERS_PER_PAGE);

    private void Start()
    {
        GenerateTable();
        InitializeHeaders();

        showExpectWinrate = true;
    }

    private void Update()
    {
        CheckScrollDown();
    }

    //========================================================
    // Generate
    //========================================================

    public void GenerateTable()
    {
        displayCharacters.Clear();
        rankMap.Clear();

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            if (character != null) displayCharacters.Add(character);

            CacheExpectedWinRate(character);
        }

        currentStartIndex = 0;

        EnsureRowCount();
        ArrangeTable(currentSortItem, currentDirection);
    }

    private void EnsureRowCount()
    {
        int requiredCount = Mathf.Min(CHARACTERS_PER_PAGE, displayCharacters.Count);

        while (rowList.Count < requiredCount)
        {
            CharacterRowUI row = Instantiate(rowPrefab, content);
            row.Initialize(this);
            row.gameObject.SetActive(false);

            rowList.Add(row);
        }
    }

    private void InitializeHeaders()
    {
        foreach (CharacterTableHeaderUI header in headers)
            header.Initialize(this);
    }

    //========================================================
    // Refresh
    //========================================================

    public void RefreshTable()
    {
        RefreshCharacterPage();
    }

    public void ReArrangetable()
    {
        ArrangeTable(currentSortItem, currentDirection);
    }

    public void SetAllOld()
    {
        foreach (CharacterRowUI row in rowList)
        {
            row.isNewCharacter = false;
        }
    }

    //========================================================
    // Page
    //========================================================

    private void RefreshCharacterPage()
    {
        currentStartIndex = Mathf.Clamp(currentStartIndex, 0, MaxStartIndex);

        int endIndex = Mathf.Min(
            currentStartIndex + CHARACTERS_PER_PAGE,
            displayCharacters.Count
        );

        int visibleCount = endIndex - currentStartIndex;

        for (int i = 0; i < rowList.Count; i++)
        {
            CharacterRowUI row = rowList[i];

            if (i < visibleCount)
            {
                RuntimeCharacter character = displayCharacters[currentStartIndex + i];

                int rank = rankMap.TryGetValue(character, out int value)
                    ? value
                    : 0;

                row.gameObject.SetActive(true);
                row.SetCharacter(character, rank);
                if (row.isNewCharacter)
                {
                    if (expectedWinRateCache.TryGetValue(character, out float expectedWinRate))
                    {
                        row.UpdateExpectedWinRate(expectedWinRate);
                        Debug.Log($"{row.RuntimeCharacter.OriginCharacter.characterName} : {expectedWinRate}");
                    }
                }
            }
            else
            {
                row.gameObject.SetActive(false);
            }
        }

    }


    private void CheckScrollDown()
    {
        float scrollInput = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scrollInput) < 0.01f)
            return;

        if (Time.unscaledTime - previousScrollTime < SCROLL_INPUT_INTERVAL)
            return;

        previousScrollTime = Time.unscaledTime;

        if (scrollInput < 0)
            currentStartIndex++;
        else
            currentStartIndex--;

        currentStartIndex = Mathf.Clamp(currentStartIndex, 0, MaxStartIndex);

        RefreshCharacterPage();
    }

    //========================================================
    // Pin
    //========================================================

    public void TogglePinCharacter(CharacterRowUI row)
    {
        if (row == null || row.RuntimeCharacter == null)
            return;

        Character originCharacter = row.RuntimeCharacter.OriginCharacter;

        if (pinnedOriginCharacter == originCharacter)
        {
            pinnedOriginCharacter = null;
        }
        else
        {
            pinnedOriginCharacter = originCharacter;
        }

        // 핀 상태가 바뀌면 첫 캐릭터부터 표시
        currentStartIndex = 0;

        ArrangeTable(currentSortItem, currentDirection);
    }

    public bool IsPinnedCharacter(RuntimeCharacter character)
    {
        return character != null &&
               pinnedOriginCharacter == character.OriginCharacter;
    }

    //========================================================
    // Winrate Expect
    //========================================================

    public void CacheExpectedWinRate(RuntimeCharacter character)
    {
        if (character == null || expectedWinRateCache.ContainsKey(character))
            return;

        if (BattlePredictor.Instance == null)
        {
            Debug.LogError("BattlePredictor가 초기화되지 않았습니다.");
            return;
        }

        float winRate = BattlePredictor.Instance.PredictBattle(character, PlayerTier.Gold);

        expectedWinRateCache.Add(character, winRate);
    }

    //========================================================
    // Arrange
    //========================================================

    private void ArrangeTable(AnalysisItem item, SortDirection direction)
    {
        List<RuntimeCharacter> sortedCharacters =
            AnalysisManager.Instance.GetSortedCharacters(item, direction);

        rankMap.Clear();

        for (int i = 0; i < sortedCharacters.Count; i++)
            rankMap[sortedCharacters[i]] = i + 1;

        displayCharacters.Clear();

        // 고정 캐릭터는 화면에서만 가장 먼저 표시
        if (pinnedOriginCharacter != null)
        {
            foreach (RuntimeCharacter character in sortedCharacters)
            {
                if (character.OriginCharacter == pinnedOriginCharacter)
                {
                    displayCharacters.Add(character);
                    break;
                }
            }
        }

        // 나머지는 정렬 순서대로 표시
        foreach (RuntimeCharacter character in sortedCharacters)
        {
            if (pinnedOriginCharacter != null &&
                character.OriginCharacter == pinnedOriginCharacter)
            {
                continue;
            }

            displayCharacters.Add(character);
        }

        EnsureRowCount();

        currentStartIndex = Mathf.Clamp(currentStartIndex, 0, MaxStartIndex);

        RefreshCharacterPage();
    }

    //========================================================
    // Symbol
    //========================================================

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

    //========================================================
    // Header
    //========================================================

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

        currentStartIndex = 0;

        ArrangeTable(currentSortItem, currentDirection);

        foreach (CharacterTableHeaderUI header in headers)
        {
            header.Refresh(
                currentSortItem == header.Item,
                currentDirection
            );
        }
    }
}
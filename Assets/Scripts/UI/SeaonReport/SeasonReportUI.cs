using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonReportUI : MonoBehaviour
{
    public static SeasonReportUI Instance;

    private const int CHARACTERS_PER_PAGE = 8;

    [Header("Character")]
    [SerializeField] private List<GameObject> CharacterRows = new();
    [SerializeField] private GameObject CharacterRowParent;
    [SerializeField] private GameObject CharacterRowPrefab;

    [Header("Report")]
    [SerializeField] private TMP_Text BalanceCheck;

    [SerializeField] private TMP_Text TrustPoint;
    [SerializeField] private TMP_Text TrustPointText;

    [SerializeField] private TMP_Text ResourcePoint;
    [SerializeField] private TMP_Text ResourcePointText;

    [Header("Page")]
    [SerializeField] private Button PreviousPageButton;
    [SerializeField] private Button NextPageButton;

    [Header("Season")]
    [SerializeField] private Button ProceedButton;

    public event System.Action OnProceed;

    public bool IsSeasonFinished;

    private int currentPage = 0;

    private int TotalPage =>
        Mathf.CeilToInt((float)CharacterRows.Count / CHARACTERS_PER_PAGE);

    private void Awake()
    {
        Instance = this;

        IsSeasonFinished = false;

        ProceedButton.onClick.AddListener(ProceedSeason);
        PreviousPageButton.onClick.AddListener(PreviousPage);
        NextPageButton.onClick.AddListener(NextPage);
    }

    public void Initialize(int currentSeason)
    {
        IsSeasonFinished = true;

        InitializeCharacterRows(currentSeason);

        currentPage = 0;
        RefreshCharacterPage();

        SetBalanceText();
        SetTrustText();
        SetResourceText();
    }

    // =========================================================
    // Character
    // =========================================================

    private void InitializeCharacterRows(int currentSeason)
    {
        ClearCharacterRows();

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            SeasonReportRowUI row = Instantiate(CharacterRowPrefab, CharacterRowParent.transform).GetComponent<SeasonReportRowUI>();

            List<CharacterStatistics> stats = StatisticsManager.Instance.GetSeasonStatistics(character.OriginCharacter.id, currentSeason);

            row.Initialize(character, stats);

            CharacterRows.Add(row.gameObject);
        }
    }

    private void ClearCharacterRows()
    {
        for (int i = CharacterRowParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(CharacterRowParent.transform.GetChild(i).gameObject);
        }

        CharacterRows.Clear();
    }

    private void RefreshCharacterPage()
    {
        int startIndex = currentPage * CHARACTERS_PER_PAGE;
        int endIndex = startIndex + CHARACTERS_PER_PAGE;

        for (int i = 0; i < CharacterRows.Count; i++)
        {
            bool isVisible =
                i >= startIndex &&
                i < endIndex;

            CharacterRows[i].SetActive(isVisible);
        }

        UpdatePageButtons();
    }

    private void UpdatePageButtons()
    {
        PreviousPageButton.interactable = currentPage > 0;
        NextPageButton.interactable = currentPage < TotalPage - 1;
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
    // Balance
    // =========================================================

    private void SetBalanceText()
    {
        DescriptionPopupUI popup = BalanceCheck.gameObject.GetComponent<DescriptionPopupUI>();

        popup.SetText("캐릭터 밸런스", BuildBalanceDescription());
    }

    private string BuildBalanceDescription()
    {
        StringBuilder builder = new();

        builder.AppendLine("<b><캐릭터 밸런스></b>");
        builder.AppendLine();

        float sumTrust = 0f;
        int characterCount = 0;

        foreach (CharacterTrustReport report in TrustManager.Instance.CharacterTrustReports)
        {
            string sign = report.trust >= 0 ? "+" : "";

            builder.AppendLine($"{report.characterName}  {sign}{report.trust:F1}");

            sumTrust += report.trust;
            characterCount++;
        }

        if (characterCount > 0)
        {
            builder.AppendLine("-----");

            builder.AppendLine($"<결과> : {sumTrust / characterCount:F0} " + $"({sumTrust:F1} / {characterCount})");
        }

        return builder.ToString();
    }

    // =========================================================
    // Trust
    // =========================================================

    private void SetTrustText()
    {
        float trust = ResourceManager.Instance.curSeasonTrust;

        TrustPointText.text = $"+ {trust:0}";

        DescriptionPopupUI popup = TrustPoint.gameObject.GetComponent<DescriptionPopupUI>();

        popup.SetText("시즌 신뢰도", GetTrustReportDescription());
    }

    private string GetTrustReportDescription()
    {
        StringBuilder builder = new();

        builder.AppendLine("<b><시즌 신뢰도></b>");
        builder.AppendLine();

        foreach (TrustReportData report in TrustManager.Instance.SeasonTrustReports)
        {
            builder.AppendLine($"{report.title}  {report.trust:+0;-0;0}");

            builder.AppendLine(report.description);
            builder.AppendLine();
        }

        int goalTrust = GetCompletedGoalTrust();

        if (goalTrust != 0)
        {
            builder.AppendLine($"목표 달성 보상  {goalTrust:+0;-0;0}");

            builder.AppendLine("달성한 목표에 따른 신뢰도 보상");

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private int GetCompletedGoalTrust()
    {
        int totalTrust = 0;

        foreach (DeveloperGoal goal in GoalManager.Instance.GetGoals)
        {
            if (!goal.IsComplete)
                continue;

            totalTrust += goal.Reward.TrustPoint;
        }

        return totalTrust;
    }

    // =========================================================
    // Resource
    // =========================================================

    private void SetResourceText()
    {
        int resource = ResourceManager.Instance.curSeasonResource;

        ResourcePointText.text = $"+ {resource:0}";

        DescriptionPopupUI popup = ResourcePoint.gameObject.GetComponent<DescriptionPopupUI>();

        popup.SetText("개발 리소스", GetResourceReportDescription());
    }

    private string GetResourceReportDescription()
    {
        StringBuilder builder = new();

        builder.AppendLine("<b><개발 리소스></b>");
        builder.AppendLine();

        foreach (TrustReportData report in TrustManager.Instance.SeasonResourceReports)
        {
            builder.AppendLine($"{report.title}  {report.trust:+0;-0;0}");

            builder.AppendLine(report.description);
            builder.AppendLine();
        }

        builder.AppendLine($"목표 달성 보상 : +{GetCompletedGoalResource()}");

        return builder.ToString();
    }

    private int GetCompletedGoalResource()
    {
        int totalResource = 0;

        foreach (DeveloperGoal goal in GoalManager.Instance.GetGoals)
        {
            if (!goal.IsComplete)
                continue;

            totalResource += goal.Reward.DevelopResource;
        }

        return totalResource;
    }

    // =========================================================
    // Season
    // =========================================================

    private void ProceedSeason()
    {
        if (IsSeasonFinished)
        {
            IsSeasonFinished = false;

            SeasonManager.Instance.NextSeason();
        }

        OnProceed?.Invoke();
    }
}
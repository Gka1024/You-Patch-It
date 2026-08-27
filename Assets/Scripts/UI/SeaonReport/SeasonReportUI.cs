using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonReportUI : MonoBehaviour
{
    public static SeasonReportUI Instance;

    [SerializeField] private List<GameObject> CharacterRows;
    [SerializeField] private GameObject CharacterRowParent;
    [SerializeField] private GameObject CharacterRowPrefab;

    [SerializeField] private TMP_Text BalanceCheck;

    [SerializeField] private TMP_Text TrustPoint;
    [SerializeField] private TMP_Text TrustPointText;

    [SerializeField] private TMP_Text ResourcePoint;
    [SerializeField] private TMP_Text ResourcePointText;
    [SerializeField] private Button ProceedButton;

    public event System.Action OnProceed;
    public bool IsSeasonFinished;

    void Awake()
    {
        Instance = this;
        IsSeasonFinished = false;
        ProceedButton.onClick.AddListener(ProceedSeason);
    }

    public void Initialize(int currentSeason)
    {
        IsSeasonFinished = true;

        for (int i = CharacterRowParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(CharacterRowParent.transform.GetChild(i).gameObject);
        }

        CharacterRows.Clear();

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            SeasonReportRowUI row = Instantiate(CharacterRowPrefab, CharacterRowParent.transform).GetComponent<SeasonReportRowUI>();
            List<CharacterStatistics> stats = StatisticsManager.Instance.GetSeasonStatistics(character.OriginCharacter.id, currentSeason);
            row.Initialize(character, stats);
            CharacterRows.Add(row.gameObject);
        }

        RuntimeCharacter addCharacter = RuntimeCharacterManager.Instance.AddedRuntimeCharacter;

        SetBalanceText();
        SetTrustText();
        SetResourceText();
    }

    // ======== 밸런스

    private void SetBalanceText()
    {
        string desc = BuildBalanceDescription();
        BalanceCheck.gameObject.GetComponent<DescriptionPopupUI>().SetText("캐릭터 밸런스", desc);
    }

    private string BuildBalanceDescription()
    {
        System.Text.StringBuilder builder = new();

        builder.AppendLine("<b> <캐릭터 밸런스> </b>");

        float sumtrust = 0;
        int characterCount = 0;

        foreach (CharacterTrustReport report in TrustManager.Instance.CharacterTrustReports)
        {
            string sign = report.trust >= 0 ? "+" : " -";

            builder.AppendLine($"{report.characterName}  {sign}{report.trust:F1}");
            characterCount++;
            sumtrust += report.trust;
        }

        builder.AppendLine($"-----");
        builder.AppendLine($"<결과> : {sumtrust / characterCount:F0} ({sumtrust:F1} / {characterCount}) ");

        return builder.ToString();
    }

    // ========= 신뢰도

    private void SetTrustText()
    {
        float trust = ResourceManager.Instance.curSeasonTrust;

        TrustPointText.text = $"+ {trust:0}";

        DescriptionPopupUI popup =
            TrustPoint.gameObject.GetComponent<DescriptionPopupUI>();

        popup.SetText(
            "시즌 신뢰도",
            GetTrustReportDescription()
        );
    }

    private string GetTrustReportDescription()
    {
        System.Text.StringBuilder builder = new();

        foreach (TrustReportData report in TrustManager.Instance.SeasonTrustReports)
        {
            builder.AppendLine(
                $"{report.title}  {report.trust:+0;-0;0}"
            );

            builder.AppendLine(report.description);
            builder.AppendLine();
        }

        // 목표 달성 보상
        int goalTrust = GetCompletedGoalTrust();

        if (goalTrust != 0)
        {
            builder.AppendLine(
                $"목표 달성 보상  {goalTrust:+0;-0;0}"
            );

            builder.AppendLine(
                $"달성한 목표에 따른 신뢰도 보상"
            );

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

    // ======= 리소스

    private void SetResourceText()
    {
        int resource = ResourceManager.Instance.curSeasonResource;

        ResourcePointText.text = $"+ {resource:0}";

        DescriptionPopupUI popup =
            ResourcePoint.gameObject.GetComponent<DescriptionPopupUI>();

        popup.SetText(
            "개발 리소스",
            GetResourceReportDescription()
        );
    }

    private string GetResourceReportDescription()
    {
        System.Text.StringBuilder builder = new();

        builder.AppendLine("<b><개발 리소스></b>");
        builder.AppendLine();

        foreach (TrustReportData report in TrustManager.Instance.SeasonResourceReports)
        {
            builder.AppendLine(
                $"{report.title}  {report.trust:+0;-0;0}"
            );

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

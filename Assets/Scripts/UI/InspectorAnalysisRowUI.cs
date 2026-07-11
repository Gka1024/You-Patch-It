using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectorAnalysisRowUI : MonoBehaviour
{
    [Header("Stat")]
    [SerializeField] private AnalysisItem analysisItem;

    [Header("UI")]
    [SerializeField] private TMP_Text itemName;

    [SerializeField] private TMP_Text rankDelta;

    [SerializeField] private TMP_Text currentSeasonText;
    [SerializeField] private TMP_Text pastSeasonText;


    public void Initialize(RuntimeCharacter character)
    {
        RefreshUI(character);
    }

    private void RefreshUI(RuntimeCharacter character)
    {
        AnalysisData data = StatisticsManager.Instance.GetAnalysis(character, analysisItem);

        SetText(data);
    }

    private void SetText(AnalysisData data)
    {
        currentSeasonText.text =
            $"{FormatValue(data.CurrentValue)} (#{data.CurrentRank})";

        pastSeasonText.text = data.HasPastData ?
            $"{FormatValue(data.PastValue)} (#{data.PastRank})" : "-";

        int delta = data.PastRank - data.CurrentRank;

        if (delta > 0)
        {
            rankDelta.text = $"▲{delta}";
        }
        else if (delta < 0)
        {
            rankDelta.text = $"▼{-delta}";
        }
        else
        {
            rankDelta.text = "-";
        }
    }

    private string FormatValue(float value)
    {
        return analysisItem switch
        {
            AnalysisItem.Winrate or AnalysisItem.Pickrate => $"{value:0.0}%",
            AnalysisItem.MatchCount => $"{value:0}",
            _ => $"{value:0.##}",
        };
    }
}

public enum AnalysisItem
{
    Winrate,
    Pickrate,
    AverageDamage,
    AverageLiveTme,
    AverageDPS,
    AverageMoveDistance,
    AverageAttackCount,
    AverageSkillCount,
    MatchCount,
    
}

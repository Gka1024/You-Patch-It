using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectorStatisticRowUI : MonoBehaviour
{
    [Header("Stat")]
    [SerializeField] private AnalysisItem analysisItem;

    [Header("UI")]
    [SerializeField] private TMP_Text itemName;

    [SerializeField] private TMP_Text rankDelta;

    [SerializeField] private TMP_Text currentSeasonText;
    [SerializeField] private TMP_Text pastSeasonText;

    private const int SHOW_DPS = 1024;
    private bool isDPSUnlocked = false;

    public void Initialize(RuntimeCharacter character)
    {
        RefreshUI(character);
        if (analysisItem == AnalysisItem.AverageDPS)
        {
            UnlockManager.Instance.OnUnlockChanged += CheckUnlock;
        }
    }

    private void CheckUnlock()
    {
        if (UnlockManager.Instance.IsUnlocked(SHOW_DPS))
        {
            isDPSUnlocked = true;
            UnlockManager.Instance.OnUnlockChanged -= CheckUnlock;
        }
    }

    private void RefreshUI(RuntimeCharacter character)
    {
        AnalysisData data = AnalysisManager.Instance.GetAnalysis(character, analysisItem);

        if (analysisItem == AnalysisItem.AverageDPS)
        {
            if (isDPSUnlocked)
            {
                SetText(data);
            }
            else
            {
                SetTextEmpty();
            }
        }
        else
        {
            SetText(data);
        }
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

    private void SetTextEmpty()
    {
        currentSeasonText.text = $" - ";
        pastSeasonText.text = $" - ";
        rankDelta.text = "-";
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


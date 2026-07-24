using UnityEngine;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance;
    private CharacterDatabase database;

    public CharacterTableUI characterTableUI;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(CharacterDatabase database)
    {
        this.database = database;
    }


    public void GenerateResult()
    {
        Debug.Log("===== Season Result =====");

        foreach (var pair in StatisticsManager.Instance.GetAllStatistics())
        {
            Character character = database.GetCharacter(pair.Key);
            CharacterStatistics stat = pair.Value;

            float winRate = stat.WinRate;
            float averageDamage = stat.AverageDamage;
            float averageSurvivalTime = stat.AverageSurvivalTime;
            float totalDamage = stat.TotalDamage;

            Debug.Log(
                $"{character.name} | " +
                $"WinRate : {winRate:F1}% | " +
                $"Match : {stat.MatchCount} | " +
                $"Damage : {averageDamage:F1} | " +
                $"DamageT : {totalDamage:F1} | " +
                $"Survival : {averageSurvivalTime:F1}s");
        }

        characterTableUI.RefreshTable();

        SeasonManager.Instance.FinishResult();
    }
}
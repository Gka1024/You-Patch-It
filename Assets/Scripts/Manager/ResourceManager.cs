using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Resource")]
    [SerializeField] private int trustExp;
    [SerializeField] private int developResource;

    [SerializeField] private float trust;

    public int TrustPoint => Mathf.RoundToInt(trust);
    public int DevelopResource => developResource;

    private void Awake()
    {
        Instance = this;
        trust = 50f;
        developResource = 200;
    }

    //====================================================
    // Public
    //====================================================

    public void GiveReward()
    {
        EvaluateSeason();
        SeasonManager.Instance.FinishReward();
    }

    public void EvaluateSeason()
    {
        int trustReward = CalculateSeasonTrust();

        AddTrust(trustReward);

        AddDevelopResource(CalculateSeasonDevelopResource());
    }

    public void AddReward(GoalReward reward)
    {
        Debug.Log(AddTrust(reward.TrustPoint));
        Debug.Log(AddDevelopResource(reward.DevelopResource));
    }

    public float AddTrust(float amount)
    {
        float multiplier = Mathf.Pow((100f - trust) / 100f, 1.5f);

        trust += amount * multiplier;
        trust = Mathf.Clamp(trust, 0f, 100f);

        return amount;
    }

    public int AddDevelopResource(int amount)
    {
        developResource += Mathf.Max(0, amount);
        return amount;
    }

    public bool SpendDevelopResource(int amount)
    {
        if (developResource < amount)
            return false;

        developResource -= amount;
        UIManager.Instance.upDisplayUI.Refresh();
        return true;
    }

    //====================================================
    // Calculation
    //====================================================

    private int CalculateSeasonTrust()
    {
        int trust = 0;
        int temp = 0;

        temp = EvaluateWinRate();
        Debug.Log($"WinratePoint : {temp}");
        trust += temp;

        temp = EvaluateCharacterIdentity();
        Debug.Log($"EvaluateCharacterIdentity : {temp}");
        trust += temp;

        temp = EvaluateMetaDiversity();
        Debug.Log($"EvaluateMetaDiversity : {temp}");
        trust += temp;

        return trust;
    }

    private int CalculateSeasonDevelopResource()
    {
        // 신뢰도가 높을수록 개발 리소스 증가
        return Mathf.RoundToInt(20 + TrustPoint * 0.5f);
    }

    //====================================================
    // Trust Curve
    //====================================================

    private int CalculateTrustPoint(int exp)
    {
        return Mathf.Clamp(
            Mathf.FloorToInt(Mathf.Sqrt(exp)),
            0,
            100);
    }

    //====================================================
    // Evaluation
    //====================================================

    private int EvaluateWinRate()
    {
        var characters = RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        if (characters.Count == 0)
            return 0;

        float totalScore = 0f;

        foreach (RuntimeCharacter character in characters)
        {
            CharacterStatistics stat =
                StatisticsManager.Instance.GetCurrentStatistics(character);

            float delta = Mathf.Abs(stat.WinRate - 50f);

            // Score = 10 * (1 - (delta / 50)^2)
            float score =
                10f *
                (1f - Mathf.Pow(delta / 50f, 2f));

            totalScore += Mathf.Clamp(score, 0f, 10f);
        }

        return Mathf.RoundToInt(totalScore / characters.Count);
    }

    private int EvaluateCharacterIdentity()
    {
        List<RuntimeCharacter> characters =
            RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        if (characters.Count < 2)
            return 10;

        float totalDistance = 0f;
        int pairCount = 0;

        for (int i = 0; i < characters.Count; i++)
        {
            for (int j = i + 1; j < characters.Count; j++)
            {
                totalDistance += GetCharacterDistance(characters[i], characters[j]);
                pairCount++;
            }
        }

        float averageDistance = totalDistance / pairCount;

        // 평균 거리가 1.5 이하면 0점
        // 평균 거리가 4.5 이상이면 10점
        float score = Mathf.InverseLerp(1.5f, 4.5f, averageDistance);
        return Mathf.RoundToInt(score * 10f);
    }

    private float GetCharacterDistance(RuntimeCharacter a, RuntimeCharacter b)
    {
        float distance = 0f;

        distance += Mathf.Abs(a.GetStat(CharacterStatType.Attack) - b.GetStat(CharacterStatType.Attack)) / 100f;
        distance += Mathf.Abs(a.GetStat(CharacterStatType.Health) - b.GetStat(CharacterStatType.Health)) / 500f;
        distance += Mathf.Abs(a.GetStat(CharacterStatType.Defence) - b.GetStat(CharacterStatType.Defence)) / 50f;
        distance += Mathf.Abs(a.GetStat(CharacterStatType.MoveSpeed) - b.GetStat(CharacterStatType.MoveSpeed)) / 3f;
        distance += Mathf.Abs(a.GetStat(CharacterStatType.AttackSpeed) - b.GetStat(CharacterStatType.AttackSpeed)) / 2f;
        distance += Mathf.Abs(a.GetStat(CharacterStatType.AttackRange) - b.GetStat(CharacterStatType.AttackRange)) / 5f;

        return distance;
    }

    private int EvaluateMetaDiversity()
    {
        List<RuntimeCharacter> characters = RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        float average = 100f / characters.Count;

        float variance = 0;

        foreach (var c in characters)
        {
            float pick = AnalysisManager.Instance.GetPickRate(c);

            variance += Mathf.Pow(pick - average, 2);
        }

        variance /= characters.Count;

        float std = Mathf.Sqrt(variance);

        float score = Mathf.InverseLerp(15f, 0f, std);

        return Mathf.RoundToInt(score * 10);
    }
}
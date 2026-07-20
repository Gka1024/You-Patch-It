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

    public void AddTrust(float amount)
    {
        float multiplier = Mathf.Pow((100f - trust) / 100f, 1.5f);

        trust += amount * multiplier;
        trust = Mathf.Clamp(trust, 0f, 100f);
    }

    public void AddDevelopResource(int amount)
    {
        developResource += Mathf.Max(0, amount);
    }

    public bool SpendDevelopResource(int amount)
    {
        if (developResource < amount)
            return false;

        developResource -= amount;
        return true;
    }

    //====================================================
    // Calculation
    //====================================================

    private int CalculateSeasonTrust()
    {
        int trust = 0;

        trust += EvaluateWinRate();
        trust += EvaluateDeveloperGoal();
        trust += EvaluateCharacterIdentity();
        trust += EvaluateMetaDiversity();

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

    private int EvaluateDeveloperGoal()
    {
        // TODO
        return 0;
    }

    private int EvaluateCharacterIdentity()
    {
        // TODO
        // 캐릭터들이 지나치게 비슷해지면 감점
        return 0;
    }

    private int EvaluateMetaDiversity()
    {
        // TODO
        // 픽률 분산, 역할 다양성 등
        return 0;
    }
}
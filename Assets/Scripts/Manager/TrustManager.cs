using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrustManager : MonoBehaviour
{
    public static TrustManager Instance;

    private bool Is3vs3Unlocked = false;

    [SerializeField] private List<TierWeight> tierWeights = new();

    private readonly List<TrustReportData> seasonTrustReports = new();
    public IReadOnlyList<TrustReportData> SeasonTrustReports => seasonTrustReports;

    private readonly List<CharacterTrustReport> characterTrustReports = new();
    public IReadOnlyList<CharacterTrustReport> CharacterTrustReports => characterTrustReports;

    private readonly List<TrustReportData> seasonResourceReports = new();
    public IReadOnlyList<TrustReportData> SeasonResourceReports => seasonResourceReports;

    private void Awake()
    {
        Instance = this;
        InitializeTierWeights();
    }

    public void CalculateTrust()
    {
        Debug.Log("Calculate Trust Start");

        int developResource = CalculateSeasonDevelopResource();
        float trust = CalculateSeasonTrust();

        ResourceManager.Instance.GiveSeasonReward(developResource, trust);

        Debug.Log($"Reward : +{developResource} Develop / {trust:+0;-0;0} Trust");
    }

    //====================================================
    // Calculation
    //====================================================

    private int CalculateSeasonDevelopResource()
    {
        seasonResourceReports.Clear();

        int baseResource;
        float trustMultiplier;

        if (!Is3vs3Unlocked)
        {
            baseResource = 30;
            trustMultiplier = 0.9f;
        }
        else
        {
            baseResource = 50;
            trustMultiplier = 1.2f;
        }

        float trustPoint = ResourceManager.Instance.TrustPoint;
        int trustResource = Mathf.RoundToInt(trustPoint * trustMultiplier);

        seasonResourceReports.Add(
            new TrustReportData(
                "기본 지급",
                baseResource,
                "시즌 종료에 따른 기본 개발 리소스"
            )
        );

        seasonResourceReports.Add(
            new TrustReportData(
                "신뢰도 보너스",
                trustResource,
                $"현재 신뢰도 {trustPoint:0} x {trustMultiplier:0.0}"
            )
        );

        return baseResource + trustResource;
    }
    
    private int CalculateSeasonTrust()
    {
        seasonTrustReports.Clear();

        int trust = 0;

        // 승률

        int winrateScore = EvaluateWinRate();

        seasonTrustReports.Add(
            new TrustReportData("캐릭터 밸런스", winrateScore, GetTrustDescription("캐릭터들의 승률이 50%에 가까울수록 높은 평가를 받습니다.",
                    winrateScore)
            )
        );

        trust += winrateScore;

        // 캐릭터 아이덴티티

        int identityScore = EvaluateCharacterIdentity();

        seasonTrustReports.Add(
                        new TrustReportData("캐릭터 개성", identityScore, GetTrustDescription("캐릭터마다 서로 다른 능력치를 가지고 있을수록 높은 평가를 받습니다.",
                    identityScore)
            )
        );

        trust += identityScore;

        // 메타 다양성

        int metaScore = EvaluateMetaDiversity();

        seasonTrustReports.Add(
            new TrustReportData("메타 다양성", metaScore, GetTrustDescription("특정 캐릭터에 픽률이 집중되지 않을수록 높은 평가를 받습니다.",
               metaScore)
            )
        );

        trust += metaScore;

        // 직업군 밸런스

        int roleScore = EvaluateRoleBalance();

        seasonTrustReports.Add(
            new TrustReportData("직업군 밸런스", roleScore, GetTrustDescription("각 직업군의 평균 승률이 균형을 이룰수록 높은 평가를 받습니다.",
                    roleScore)
            )
        );

        trust += roleScore;

        return trust;
    }

    private string GetTrustDescription(string baseDescription, float score)
    {
        if (score > 3)
            return $"{baseDescription}\n : 긍정적";

        if (score < -3)
            return $"{baseDescription}\n : 개선 필요";

        return $"{baseDescription}\n : 보통";
    }

    //====================================================
    // Evaluation
    //====================================================

    private int EvaluateWinRate()
    {
        List<RuntimeCharacter> characters = RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        characterTrustReports.Clear();

        if (characters.Count == 0)
            return 0;

        float totalScore = 0f;
        float totalWeight = 0f;

        foreach (RuntimeCharacter character in characters)
        {
            CharacterStatistics stat = StatisticsManager.Instance.GetCurrentStatistics(character);

            float characterScore = 0f;
            float characterWeight = 0f;

            foreach (PlayerTier tier in Enum.GetValues(typeof(PlayerTier)))
            {
                TierStatistics tierStat = stat.TierStatistics[tier];

                if (tierStat.MatchCount <= 0)
                    continue;

                float score = CalculateWinRateScore(tierStat.WinRate);
                score = Mathf.Clamp(score, -20f, 10f);

                float tierWeight = GetTierWeight(tier);
                float sampleWeight = Mathf.Sqrt(tierStat.WinCount + tierStat.LoseCount);

                float weight = tierWeight * sampleWeight;

                totalScore += score * weight;
                totalWeight += weight;

                characterScore += score * weight;
                characterWeight += weight;
            }

            // 해당 캐릭터의 최종 기여도
            float finalCharacterScore = characterWeight <= 0f ? 0f : characterScore / characterWeight;

            characterTrustReports.Add(new CharacterTrustReport(character, finalCharacterScore));
        }

        if (totalWeight <= 0f)
            return 0;

        float finalScore = totalScore / totalWeight;

        return Mathf.RoundToInt(finalScore);
    }

    private float GetTierWeight(PlayerTier tier)
    {
        TierWeight tierWeight = tierWeights.FirstOrDefault(x => x.tier == tier);

        return tierWeight != null ? Mathf.Max(0f, tierWeight.weight) : 1f;
    }

    private void InitializeTierWeights()
    {
        if (tierWeights.Count > 0)
            return;

        tierWeights = new List<TierWeight>
    {
        new TierWeight { tier = PlayerTier.Bronze, weight = 1.0f },
        new TierWeight { tier = PlayerTier.Silver, weight = 1.1f },
        new TierWeight { tier = PlayerTier.Gold, weight = 1.25f },
        new TierWeight { tier = PlayerTier.Platinum, weight = 1.5f },
        new TierWeight { tier = PlayerTier.Diamond, weight = 1.75f },
        new TierWeight { tier = PlayerTier.Master, weight = 2.0f },
        new TierWeight { tier = PlayerTier.Challenger, weight = 2.5f }
    };
    }

    private float CalculateWinRateScore(float winRate)
    {
        float delta = Mathf.Abs(winRate - 50f);

        if (delta <= 10f)
            return 10f - delta;

        if (delta <= 20f)
            return Mathf.Lerp(0f, -4f, (delta - 10f) / 10f);

        if (delta <= 30f)
            return Mathf.Lerp(-4f, -7f, (delta - 20f) / 10f);

        if (delta <= 40f)
            return Mathf.Lerp(-7f, -13f, (delta - 30f) / 10f);

        return Mathf.Lerp(-13f, -20f, (delta - 40f) / 10f);
    }

    // ----------------------------------------

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
                totalDistance += GetCharacterDistance(
                    characters[i],
                    characters[j]);

                pairCount++;
            }
        }

        float averageDistance = totalDistance / pairCount;

        float score = CalculateIdentityScore(averageDistance);

        Debug.Log(
            $"Character Identity | " +
            $"Average Distance : {averageDistance:F2} | " +
            $"Score : {score:F1}");

        return Mathf.RoundToInt(score);
    }

    private float GetCharacterDistance(RuntimeCharacter a, RuntimeCharacter b)
    {
        float distance = 0;

        distance += Mathf.Abs(a.GetStat(CharacterStatType.Attack) - b.GetStat(CharacterStatType.Attack)) / 100f;
        distance += Mathf.Abs(a.GetStat(CharacterStatType.Health) - b.GetStat(CharacterStatType.Health)) / 500f;
        distance += Mathf.Abs(a.GetStat(CharacterStatType.Defence) - b.GetStat(CharacterStatType.Defence)) / 50f;
        distance += Mathf.Abs(a.GetStat(CharacterStatType.MoveSpeed) - b.GetStat(CharacterStatType.MoveSpeed)) / 3f;
        distance += Mathf.Abs(a.GetStat(CharacterStatType.AttackSpeed) - b.GetStat(CharacterStatType.AttackSpeed)) / 2f;
        distance += Mathf.Abs(a.GetStat(CharacterStatType.AttackRange) - b.GetStat(CharacterStatType.AttackRange)) / 5f;

        return distance;
    }

    private float CalculateIdentityScore(float distance)
    {
        // 캐릭터가 거의 동일하면 강한 페널티
        if (distance <= 1f)
        {
            return Mathf.Lerp(-20f, -8f, distance / 1f);
        }

        // 1 ~ 2
        if (distance <= 2f)
        {
            return Mathf.Lerp(-8f, 2f, (distance - 1f) / 1f);
        }

        // 2 ~ 3
        if (distance <= 3f)
        {
            return Mathf.Lerp(2f, 7f, (distance - 2f) / 1f);
        }

        // 3 ~ 4
        if (distance <= 4f)
        {
            return Mathf.Lerp(7f, 9f, (distance - 3f) / 1f);
        }

        // 4 이상이면 거의 만점
        return 10f;
    }

    // ----------------------------------------

    private int EvaluateMetaDiversity()
    { // 모든 캐릭터의 픽률에 기반한 신뢰도 평가
        List<RuntimeCharacter> characters =
            RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        float average = 100f / characters.Count;

        float variance = 0;

        foreach (RuntimeCharacter character in characters)
        {
            float pick =
                AnalysisManager.Instance.GetPickRate(character);

            variance += Mathf.Pow(pick - average, 2);
        }

        variance /= characters.Count;

        float std = Mathf.Sqrt(variance);

        float score =
            Mathf.InverseLerp(
                15f,
                0f,
                std);

        return Mathf.RoundToInt(score * 10f);
    }

    // ----------------------------------------

    private int EvaluateRoleBalance()
    {
        int score = 0;

        foreach (CharacterRole role in Enum.GetValues(typeof(CharacterRole)))
        {
            List<RuntimeCharacter> characters = RuntimeCharacterManager.Instance.GetCharactersInRole(role).ToList();

            if (characters.Count == 0)
                continue;

            float totalWinrate = 0f;
            int evaluatedCount = 0;

            foreach (RuntimeCharacter character in characters)
            {
                CharacterStatistics stat = StatisticsManager.Instance.GetCurrentStatistics(character);

                // 전투 기록이 없으면 평가에서 제외
                if (stat.MatchCount <= 0) continue;

                totalWinrate += stat.Winrate;
                evaluatedCount++;
            }

            // 해당 직업군에 실제 전투 기록이 없으면 평가하지 않음
            if (evaluatedCount == 0)
                continue;

            float averageWinrate =
                totalWinrate / evaluatedCount;

            // 45~55는 정상
            if (averageWinrate >= 45f &&
                averageWinrate <= 55f)
                continue;

            float delta =
                averageWinrate > 55f
                ? averageWinrate - 55f
                : 45f - averageWinrate;

            float penalty =
                Mathf.Pow(delta / 5f, 2f);

            score -= Mathf.RoundToInt(penalty);
        }

        return Mathf.Clamp(score, -20, 10);
    }
}

[Serializable]
public class TierWeight
{
    public PlayerTier tier;
    public float weight = 1f;
}

[Serializable]
public class TrustReportData
{
    public string title;
    public float trust;
    public string description;

    public TrustReportData(string title, float trust, string description)
    {
        this.title = title;
        this.trust = trust;
        this.description = description;
    }
}

[Serializable]
public class CharacterTrustReport
{
    public int characterId;
    public string characterName;
    public float trust;

    public CharacterTrustReport(
        RuntimeCharacter character,
        float trust)
    {
        characterId = character.OriginCharacter.id;
        characterName = character.OriginCharacter.characterName;
        this.trust = trust;
    }
}
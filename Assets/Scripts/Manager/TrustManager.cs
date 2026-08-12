using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrustManager : MonoBehaviour
{
    public static TrustManager Instance;

    [SerializeField] private List<TierWeight> tierWeights = new();

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
        return Mathf.RoundToInt(
            20 +
            ResourceManager.Instance.TrustPoint * 0.5f);
    }

    private int CalculateSeasonTrust()
    {
        int trust = 0;
        int temp;

        temp = EvaluateWinRate();
        Debug.Log($"Winrate : {temp}");
        trust += temp;

        temp = EvaluateCharacterIdentity();
        Debug.Log($"Identity : {temp}");
        trust += temp;

        temp = EvaluateMetaDiversity();
        Debug.Log($"Meta : {temp}");
        trust += temp;

        temp = EvaluateRoleBalance();
        Debug.Log($"Balance : {temp}");
        trust += temp;

        return trust;
    }

    //====================================================
    // Evaluation
    //====================================================

    private int EvaluateWinRate()
    { // 각 캐릭터의 티어별 승률을 기준으로 신뢰도 평가
        List<RuntimeCharacter> characters = RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        if (characters.Count == 0)
            return 0;

        float totalScore = 0f;
        float totalWeight = 0f;

        foreach (RuntimeCharacter character in characters)
        {
            CharacterStatistics stat = StatisticsManager.Instance.GetCurrentStatistics(character);

            foreach (PlayerTier tier in Enum.GetValues(typeof(PlayerTier)))
            {
                TierStatistics tierStat = stat.TierStatistics[tier];

                // 해당 티어에서 전투가 없으면 평가하지 않음
                if (tierStat.MatchCount <= 0)
                    continue;

                // ----------------------------------------
                // 승률 -> 0 ~ 10점
                // ----------------------------------------

                float score = CalculateWinRateScore(tierStat.WinRate);

                score = Mathf.Clamp(score, -20f, 10f);

                // ----------------------------------------
                // 티어 가중치
                // ----------------------------------------

                float tierWeight = GetTierWeight(tier);

                // ----------------------------------------
                // 표본 수 보정
                //
                // 경기 수가 많을수록 신뢰도 증가
                // 단, sqrt를 사용해서 완만하게 증가
                // ----------------------------------------

                float sampleWeight = Mathf.Sqrt(tierStat.WinCount + tierStat.LoseCount);

                float weight = tierWeight * sampleWeight;

                // ----------------------------------------
                // 가중 평균
                // ----------------------------------------

                totalScore += score * weight;
                totalWeight += weight;

                Debug.Log(
    $"Character : {character.OriginCharacter.characterName} | " +
    $"Tier : {tier} | " +
    $"WinRate : {tierStat.WinRate:F1}% | " +
    $"Score : {score:F2} | " +
    $"TierWeight : {tierWeight:F2} | " +
    $"SampleWeight : {sampleWeight:F2} | " +
    $"Weight : {weight:F2} | " +
    $"CumulativeScore : {(totalScore / totalWeight):F2}"
);
            }
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
    { // 특정 직업군 편향에 따른 신뢰도 평가
        int score = 0;

        foreach (CharacterRole role in Enum.GetValues(typeof(CharacterRole)))
        {
            List<RuntimeCharacter> characters =
                RuntimeCharacterManager.Instance
                .GetCharactersInRole(role)
                .ToList();

            if (characters.Count == 0)
                continue;

            float averageWinrate = 0;

            foreach (RuntimeCharacter character in characters)
            {
                CharacterStatistics stat =
                    StatisticsManager.Instance.GetCurrentStatistics(character);

                averageWinrate += stat.WinRate;
            }

            averageWinrate /= characters.Count;

            // 45~55는 정상
            if (averageWinrate >= 45f &&
                averageWinrate <= 55f)
                continue;

            float delta =
                averageWinrate > 55f
                ? averageWinrate - 55f
                : 45f - averageWinrate;

            // 기하급수적으로 증가
            float penalty =
                Mathf.Pow(delta / 5f, 2f);

            score -= Mathf.RoundToInt(penalty);
        }

        return score;
    }
}

[Serializable]
public class TierWeight
{
    public PlayerTier tier;
    public float weight = 1f;
}
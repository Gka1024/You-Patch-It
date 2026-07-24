using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrustManager : MonoBehaviour
{
    public static TrustManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CalculateTrust()
    {
        Debug.Log("Calculate Trust Start");

        int developResource = CalculateSeasonDevelopResource();
        float trust = CalculateSeasonTrust();

        ResourceManager.Instance.GiveSeasonReward(
            developResource,
            trust);

        Debug.Log(
            $"Reward : +{developResource} Develop / {trust:+0;-0;0} Trust");
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
    {
        var characters = RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        if (characters.Count == 0)
            return 0;

        float total = 0;

        foreach (RuntimeCharacter character in characters)
        {
            CharacterStatistics stat =
                StatisticsManager.Instance.GetCurrentStatistics(character);

            float delta = Mathf.Abs(stat.WinRate - 50f);

            float score =
                10f *
                (1f - Mathf.Pow(delta / 50f, 2f));

            total += Mathf.Clamp(score, 0f, 10f);
        }

        return Mathf.RoundToInt(total / characters.Count);
    }

    private int EvaluateCharacterIdentity()
    {
        List<RuntimeCharacter> characters =
            RuntimeCharacterManager.Instance.GetAllCharacters().ToList();

        if (characters.Count < 2)
            return 10;

        float totalDistance = 0;
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

        float score =
            Mathf.InverseLerp(
                1.5f,
                4.5f,
                averageDistance);

        return Mathf.RoundToInt(score * 10f);
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

    private int EvaluateMetaDiversity()
    {
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

    private int EvaluateRoleBalance()
    {
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
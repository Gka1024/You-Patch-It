using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuntimePlayer
{
    public PlayerProfile OriginalProfile { get; }
    public PlayerTier Tier { get; }

    //========================================================
    // Combat
    //========================================================

    public float ReactionTime { get; private set; }
    public float Consistency { get; private set; }
    public float ExecutionSkill { get; private set; }
    public float DecisionAccuracy { get; private set; }

    //========================================================
    // Knowledge
    //========================================================

    public float MetaKnowledge { get; private set; }

    //========================================================
    // Play Style
    //========================================================

    public float MetaDependence { get; private set; }
    public float RiskTaking { get; private set; }

    public PreferenceShape PreferenceShape { get; private set; }

    public Dictionary<CharacterRole, float> ClassPreferences { get; } = new();

    public RuntimePlayer(PlayerProfile profile, TierSetting tier, System.Random random)
    {
        OriginalProfile = profile;
        Tier = tier.tier;

        GenerateCombat(tier, random);
        GenerateKnowledge(tier, random);
        GeneratePlayStyle(profile, random);
        GeneratePreferences(profile, tier, random);
    }

    //========================================================
    // Combat
    //========================================================

    private void GenerateCombat(TierSetting tier, System.Random random)
    {
        ReactionTime = RandomizeNormal(tier.reactionTime, tier.combatVariance, random);

        Consistency = RandomizeNormal(tier.consistency, tier.combatVariance, random);

        ExecutionSkill = RandomizeNormal(tier.executionSkill, tier.combatVariance, random);

        DecisionAccuracy = RandomizeNormal(tier.decisionAccuracy, tier.combatVariance, random);
    }

    //========================================================
    // Knowledge
    //========================================================

    private void GenerateKnowledge(TierSetting tier, System.Random random)
    {
        MetaKnowledge = RandomizeNormal(tier.metaKnowledge, tier.knowledgeVariance, random);
    }

    //========================================================
    // Play Style
    //========================================================

    private void GeneratePlayStyle(PlayerProfile profile, System.Random random)
    {
        MetaDependence = RandomizeNormal(profile.metaDependence, 5f, random);

        RiskTaking = RandomizeNormal(profile.riskTaking, 5f, random);
    }

    //========================================================
    // Preference
    //========================================================

    private void GeneratePreferences(PlayerProfile profile, TierSetting tier, System.Random random)
    {
        ClassPreferences.Clear();

        PreferenceShape = profile.preferenceShape;

        if (PreferenceShape == PreferenceShape.Custom)
        {
            LoadPreference(profile, tier, random);
        }
        else
        {
            GeneratePreferenceRandom(PreferenceShape, tier, random);
        }
    }

    private void GeneratePreferenceRandom(PreferenceShape shape, TierSetting tier, System.Random random)
    {
        List<CharacterRole> roles = Enum.GetValues(typeof(CharacterRole)).Cast<CharacterRole>().OrderBy(_ => random.Next()).ToList();

        float[] means = GetPreferenceMeans(shape);

        for (int i = 0; i < roles.Count; i++)
        {
            ClassPreferences.Add(roles[i], RandomizeNormal(means[i], tier.preferenceVariance, random));
        }
    }

    private void LoadPreference(PlayerProfile profile, TierSetting tier, System.Random random)
    {
        foreach (ClassPreference preference in profile.classPreferences)
        {
            ClassPreferences.Add(preference.characterClass, RandomizeNormal(preference.preference, tier.preferenceVariance, random));
        }
    }

    //========================================================
    // Preference Shape
    //========================================================

    private float[] GetPreferenceMeans(PreferenceShape shape)
    {
        return shape switch
        {
            PreferenceShape.OneTrick => new[]
            {
                100f,
                40f,
                30f,
                20f,
                10f,
                0f
            },

            PreferenceShape.MainTwo => new[]
            {
                90f,
                80f,
                40f,
                20f,
                20f,
                10f
            },

            PreferenceShape.Balanced => new[]
            {
                80f,
                85f,
                70f,
                60f,
                50f,
                40f
            },

            PreferenceShape.Flexible => new[]
            {
                75f,
                70f,
                68f,
                65f,
                62f,
                60f
            },

            _ => throw new ArgumentOutOfRangeException(nameof(shape))
        };
    }

    //========================================================
    // Utility
    //========================================================

    private float RandomizeNormal(float mean, float variance, System.Random random)
    {
        return Mathf.Clamp(mean + RandomOffsetNormal(variance, random), 0, 100);
    }

    private float RandomOffsetNormal(float variance, System.Random random)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();

        double standard = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

        float sigma = variance / 3f;

        return (float)(standard * sigma);
    }
}
public enum PreferenceShape
{
    Custom,
    OneTrick,
    MainTwo,
    Balanced,
    Flexible
}
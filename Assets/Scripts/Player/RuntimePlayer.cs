using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuntimePlayer
{
    public PlayerProfile originalProfile;
    public PlayerTier Tier;

    // Combat
    public float reactionTime;
    public float consistency;
    public float executionSkill;
    public float decisionAccuracy;

    // Knowledge
    public float metaKnowledge;
    public float metaDependence;
    public float experiment;

    // Personality
    public PreferenceShape preferenceShape;

    // Role
    public Dictionary<CharacterRole, float> classPreferences = new();
    public Dictionary<CharacterRole, float> classSkills = new();

    public RuntimePlayer(PlayerProfile profile, TierSetting tierSetting, System.Random random)
    {
        originalProfile = profile;
        Tier = tierSetting.tier;

        //---------------------------------------
        // 공통 재능
        //---------------------------------------

        float combatTalent = RandomOffsetNormal(tierSetting.combatVariance * 0.5f, random);

        float knowledgeTalent = RandomOffsetNormal(tierSetting.knowledgeVariance * 0.5f, random);

        //---------------------------------------
        // Combat
        //---------------------------------------

        reactionTime = RandomizeNormal(profile.reactionTime, combatTalent, tierSetting.combatVariance, random);

        consistency = RandomizeNormal(profile.consistency, combatTalent, tierSetting.combatVariance, random);

        executionSkill = RandomizeNormal(profile.executionSkill, combatTalent, tierSetting.combatVariance, random);

        decisionAccuracy = RandomizeNormal(profile.decisionAccuracy, combatTalent, tierSetting.combatVariance, random);

        //---------------------------------------
        // Knowledge
        //---------------------------------------

        metaKnowledge = RandomizeNormal(profile.metaKnowledge, knowledgeTalent, tierSetting.knowledgeVariance, random);

        metaDependence = RandomizeNormal(profile.metaDependence, knowledgeTalent, tierSetting.knowledgeVariance, random);

        experiment = RandomizeNormal(profile.experiment, knowledgeTalent, tierSetting.knowledgeVariance, random);

        //---------------------------------------
        // Role Data
        //---------------------------------------

        GenerateRoleData(profile, tierSetting, random);
    }

    //========================================================

    private void GenerateRoleData(PlayerProfile profile, TierSetting tierSetting, System.Random random)
    {
        classPreferences.Clear();
        classSkills.Clear();

        if (profile.classPreferences.Count == 0)
        {
            GeneratePreferenceRandom(tierSetting, random);
        }
        else
        {
            LoadPreference(profile, tierSetting, random);
        }

        if (profile.classSkills.Count == 0)
        {
            GenerateSkillRandom(tierSetting, random);
        }
        else
        {
            LoadSkills(profile, tierSetting, random);
        }
    }

    private void GeneratePreferenceRandom(TierSetting tierSetting, System.Random random)
    {
        preferenceShape = GeneratePreferenceShape(random);

        List<CharacterRole> roles = Enum.GetValues(typeof(CharacterRole))
            .Cast<CharacterRole>()
            .OrderBy(_ => random.Next())
            .ToList();

        float[] means = GetPreferenceMeans(preferenceShape);

        for (int i = 0; i < roles.Count; i++)
        {
            classPreferences.Add(roles[i], RandomizeNormal(means[i], 0, tierSetting.preferenceVariance, random));
        }
    }

    private void GenerateSkillRandom(TierSetting tierSetting, System.Random random)
    {
        foreach (KeyValuePair<CharacterRole, float> pair in classPreferences)
        {
            float talent = RandomizeNormal(50f, 0, tierSetting.classSkillVariance, random);

            float skill = Mathf.Clamp(pair.Value * 0.45f + talent * 0.55f + RandomOffsetNormal(3f, random), 0, 100);

            classSkills.Add(pair.Key, skill);
        }
    }

    private void LoadPreference(PlayerProfile profile, TierSetting tierSetting, System.Random random)
    {
        foreach (ClassPreference preference in profile.classPreferences)
        {
            classPreferences.Add(
                preference.characterClass,
                RandomizeNormal(
                    preference.preference,
                    0,
                    tierSetting.preferenceVariance,
                    random));
        }
    }

    private void LoadSkills(PlayerProfile profile, TierSetting tierSetting, System.Random random)
    {
        foreach (ClassSkill skill in profile.classSkills)
        {
            classSkills.Add(
                skill.characterClass,
                RandomizeNormal(
                    skill.skill,
                    0,
                    tierSetting.classSkillVariance,
                    random));
        }
    }


    private PreferenceShape GeneratePreferenceShape(System.Random random)
    {
        int roll = random.Next(100);

        if (roll < 25) return PreferenceShape.OneTrick;

        if (roll < 55) return PreferenceShape.MainTwo;

        if (roll < 85) return PreferenceShape.Balanced;

        return PreferenceShape.Flexible;
    }

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
                40f,
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

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    //========================================================

    private float RandomizeNormal(float mean, float talent, float variance, System.Random random)
    {
        float offset = RandomOffsetNormal(variance, random);

        return Mathf.Clamp(mean + talent + offset, 0f, 100f);
    }

    private float RandomOffsetNormal(float variance, System.Random random)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();

        double standard =
            Math.Sqrt(-2.0 * Math.Log(u1)) *
            Math.Cos(2.0 * Math.PI * u2);

        float sigma = variance / 3f;

        return (float)(standard * sigma);
    }
}

public enum PreferenceShape
{
    OneTrick,
    MainTwo,
    Balanced,
    Flexible
}
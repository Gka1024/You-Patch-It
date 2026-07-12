using System.Collections.Generic;
using UnityEngine;

public class RuntimePlayer
{
    public PlayerProfile originalProfile;

    // Combat
    public float reactionTime;
    public float consistency;
    public float executionSkill;
    public float decisionAccuracy;

    // Knowledge
    public float metaKnowledge;
    public float metaDependence;
    public float experiment;

    // Role
    public Dictionary<CharacterRole, float> classPreferences = new();
    public Dictionary<CharacterRole, float> classSkills = new();

    public RuntimePlayer(PlayerProfile profile, TierSetting tierSetting, System.Random random)
    {
        originalProfile = profile;

        //---------------------------------------
        // 공통 재능 
        //---------------------------------------

        float combatTalent =
            RandomOffsetNormal(tierSetting.combatVariance * 0.5f, random);

        float knowledgeTalent =
            RandomOffsetNormal(tierSetting.knowledgeVariance * 0.5f, random);

        //---------------------------------------
        // Combat
        //---------------------------------------

        reactionTime =
            RandomizeNormal(profile.reactionTime,
                combatTalent,
                tierSetting.combatVariance,
                random);

        consistency =
            RandomizeNormal(profile.consistency,
                combatTalent,
                tierSetting.combatVariance,
                random);

        executionSkill =
            RandomizeNormal(profile.executionSkill,
                combatTalent,
                tierSetting.combatVariance,
                random);

        decisionAccuracy =
            RandomizeNormal(profile.decisionAccuracy,
                combatTalent,
                tierSetting.combatVariance,
                random);

        //---------------------------------------
        // Knowledge
        //---------------------------------------

        metaKnowledge =
            RandomizeNormal(profile.metaKnowledge,
                knowledgeTalent,
                tierSetting.knowledgeVariance,
                random);

        metaDependence =
            RandomizeNormal(profile.metaDependence,
                knowledgeTalent,
                tierSetting.knowledgeVariance,
                random);

        experiment =
            RandomizeNormal(profile.experiment,
                knowledgeTalent,
                tierSetting.knowledgeVariance,
                random);

        //---------------------------------------
        // Role Preference
        //---------------------------------------

        // profile.classPreferences is a dictionary of role->value
        foreach (KeyValuePair<CharacterRole, float> kvp in profile.classPreferences)
        {
            classPreferences.Add(kvp.Key, RandomizeNormal(kvp.Value, 0f, tierSetting.preferenceVariance, random));
        }

        //---------------------------------------
        // Role Skill
        //---------------------------------------

        foreach (KeyValuePair<CharacterRole, float> kvp in profile.classSkills)
        {
            float roleTalent = RandomOffsetNormal(tierSetting.classSkillVariance * 0.5f, random);

            classSkills.Add(kvp.Key, RandomizeNormal(kvp.Value, roleTalent, tierSetting.classSkillVariance, random));
        }
    }

    //---------------------------------------------------------

    private float RandomizeNormal(float mean, float talent, float variance, System.Random random)
    {
        float offset = RandomOffsetNormal(variance, random);

        return Mathf.Clamp(mean + talent + offset, 0f, 100f);
    }

    private float RandomOffsetNormal(float variance, System.Random random)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();

        double standard = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);

        float sigma = variance / 3f;

        return (float)(standard * sigma);
    }
}
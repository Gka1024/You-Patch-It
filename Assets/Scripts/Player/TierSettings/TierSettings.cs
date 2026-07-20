using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/TierSetting")]
public class TierSetting : ScriptableObject
{
    public PlayerTier tier;

    [Header("Spawn")]
    [Min(0)] public int spawnWeight = 0;

    [Header("Combat Average")]
    public float reactionTime;
    public float consistency;
    public float executionSkill;
    public float decisionAccuracy;

    [Header("Knowledge Average")]
    public float metaKnowledge;

    [Header("Variance")]
    public float combatVariance = 10;
    public float knowledgeVariance = 10;
    public float preferenceVariance = 10;
    public float classSkillVariance = 10;
}
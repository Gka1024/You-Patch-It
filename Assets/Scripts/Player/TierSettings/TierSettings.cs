using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/TierSetting")]
public class TierSetting : ScriptableObject
{
    public PlayerTier tier;

    [Min(0)]
    public int weight;

    [Header("Variance")]
    public float combatVariance = 12;
    public float knowledgeVariance = 15;
    public float classSkillVariance = 10;
    public float preferenceVariance = 3;
}
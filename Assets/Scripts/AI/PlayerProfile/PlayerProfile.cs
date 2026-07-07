using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/PlayerProfile")]
public class PlayerProfile : ScriptableObject
{
    [Range(0, 100)] public float reactionTime;
    [Range(0, 100)] public float consistency;
    [Range(0, 100)] public float executionSkill;
    [Range(0, 100)] public float decisionAccuracy;
    [Range(0, 100)] public float metaKnowledge;
}
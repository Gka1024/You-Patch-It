using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/PlayerProfile")]
public class PlayerProfile : ScriptableObject
{
    public PlayerTier tier;

    [Header("Combat Ability")]
    public float reactionTime; // less is good
    public float consistency; // high is good
    public float executionSkill; // middle is good
    public float decisionAccuracy; // high is good

    [Header("Game Knowledge")]
    public float metaKnowledge; // high is good
    public float metaDependence; // idk
    public float experiment; // idk

    [Header("Class Skill & Preference")]
    public List<ClassPreference> classPreferences = new();
    public List<ClassSkill> classSkills = new();

}

[System.Serializable]
public class ClassPreference
{
    public CharacterRole characterClass;

    [Range(0, 100)]
    public float preference;
}

[System.Serializable]
public class ClassSkill
{
    public CharacterRole characterClass;

    [Range(0, 100)]
    public float skill;
}

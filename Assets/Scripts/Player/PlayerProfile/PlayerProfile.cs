using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/PlayerProfile")]
public class PlayerProfile : ScriptableObject
{
    [Header("Spawn")]
    [Min(0)] public int spawnWeight;

    [Header("Play Style")]

    [Range(0,100)]
    public float metaDependence;

    [Range(0,100)]
    public float experiment;

    [Header("Class Preference")]
    public List<ClassPreference> classPreferences = new();

    [Header("Class Skill")]
    public List<ClassSkill> classSkills = new();
}

[System.Serializable]
public class ClassPreference
{
    public CharacterRole characterClass;

    [Range(0,100)]
    public float preference;
}

[System.Serializable]
public class ClassSkill
{
    public CharacterRole characterClass;

    [Range(0,100)]
    public float skill;
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/PlayerProfile")]
public class PlayerProfile : ScriptableObject
{
    [Header("Spawn")]
    [Min(0)] public int spawnWeight;

    [Header("Play Style")]
    public PreferenceShape preferenceShape;

    [Range(0,100)]
    public float metaDependence;

    [Range(0,100)]
    public float riskTaking;

    [Header("Class Preference")]
    public List<ClassPreference> classPreferences = new();
}

[System.Serializable]
public class ClassPreference
{
    public CharacterRole characterClass;

    [Range(0,100)]
    public float preference;
}
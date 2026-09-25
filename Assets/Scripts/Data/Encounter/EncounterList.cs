using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Data/Encounter/EncounterList")]
public class EncounterList : ScriptableObject
{
    public List<Encounter> Encounters;
}

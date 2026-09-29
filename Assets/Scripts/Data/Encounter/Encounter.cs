using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Data/Encounter/Encounter")]
public class Encounter : ScriptableObject
{
    public int id;

    public List<EncounterResource> EncounterResources;

    public string Name;
    [TextArea] public string Description;
}

[Serializable]
public class EncounterResource
{
    public bool isNegative = true;
    public GoodsType goodsType;
    public ValueFormat valueFormat;
    public ValuePeriod valuePeriod;

    public float value;
}
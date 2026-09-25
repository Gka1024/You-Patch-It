using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Data/Encounter/Encounter")]
public class Encounter : ScriptableObject
{
    public int id;

    public GoodsType goodsType;
    public ValueType valueType;
    public ValueType2 valueType2;

    public float value;

    public string Name;
    [TextArea] public string Description;
}

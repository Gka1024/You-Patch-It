using UnityEngine;

[CreateAssetMenu(menuName ="ScriptableObject/UnlockData/Data")]
public class UnlockData : ScriptableObject
{
    public int id;

    public string unlockName;
    [TextArea] public string description;

    public UnlockCategory category;

    public int cost;
    public Sprite icon;
    public UnlockData[] prerequisites;
}

public enum UnlockCategory
{
    Patch,
    Goal,
    Information,
    Operation 
}
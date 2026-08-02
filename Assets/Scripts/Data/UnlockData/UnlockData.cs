using UnityEngine;

[CreateAssetMenu(menuName ="ScriptableObject/UnlockData/Data")]
public class UnlockData : ScriptableObject
{
    public int id;

    public string unlockName;
    [TextArea] public string description;

    public UnlockCategory category;

    public int costResource;
    public Sprite icon;
    public UnlockData[] prerequisites;
}

public enum UnlockCategory
{
    Patch,
    Goal,
    Operation 
}
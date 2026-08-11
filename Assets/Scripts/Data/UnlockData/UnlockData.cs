using UnityEngine;

[CreateAssetMenu(menuName ="ScriptableObject/Data/UnlockData/Data")]
public class UnlockData : ScriptableObject
{
    public int id;

    public string unlockName;
    [TextArea] public string description;

    public UnlockCategory category;

    public int costResource;
    public Sprite icon;
    public UnlockData[] prerequisites;
    public UnlockData NextData;
}

public enum UnlockCategory
{
    Patch,
    Goal,
    Operation 
}
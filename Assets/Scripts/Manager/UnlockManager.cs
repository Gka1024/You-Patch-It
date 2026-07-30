using System.Collections.Generic;
using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance;

    public UnlockDataBase dataBase;

    private readonly HashSet<int> unlocked = new();

    private void Awake()
    {
        Instance = this;
    }

    public bool IsUnlocked(UnlockData data)
    {
        return unlocked.Contains(data.id);
    }

    public bool CanUnlock(UnlockData data)
    {
        if (IsUnlocked(data)) return false;

        foreach (UnlockData pre in data.prerequisites)
        {
            if (!IsUnlocked(pre)) return false;
        }

        return true;
    }

    public bool Unlock(UnlockData data)
    {
        if (!CanUnlock(data)) return false;

        unlocked.Add(data.id);

        return true;
    }
}
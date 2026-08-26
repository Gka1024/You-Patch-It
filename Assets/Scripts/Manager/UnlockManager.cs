using System.Collections.Generic;
using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance;

    public UnlockDataBase dataBase;

    public event System.Action OnUnlockChanged;

    private readonly HashSet<int> unlocked = new();
    private Dictionary<int, UnlockData> unlockDictionary = new();

    private void Awake()
    {
        Instance = this;
        RegisterData();
    }

    private void RegisterData()
    {
        foreach (UnlockData data in dataBase.unlocks)
        {
            unlockDictionary.Add(data.id, data);
        }
    }

    public void UnlockAll()
    {
        foreach (UnlockData data in dataBase.unlocks)
        {
            unlocked.Add(data.id);
        }

        OnUnlockChanged?.Invoke();
    }

    public UnlockData GetUnlockData(int id)
    {
        unlockDictionary.TryGetValue(id, out UnlockData data);
        return data;
    }

    public bool IsUnlocked(UnlockData data)
    {
        return unlocked.Contains(data.id);
    }

    public bool IsUnlocked(int id)
    {
        return unlocked.Contains(id);
    }

    public bool CanUnlock(UnlockData data)
    {
        if (IsUnlocked(data)) return false;
        if (SeasonManager.Instance.CurrentSeason == 1) return false;

        foreach (UnlockData pre in data.prerequisites)
        {
            if (!IsUnlocked(pre)) return false;
        }

        if (data.costResource < 0) return false;
        if (!ResourceManager.Instance.SpendDevelopResource(data.costResource)) return false;

        return true;
    }

    public bool Unlock(UnlockData data)
    {
        Debug.Log($"Unlock : {data.unlockName}");

        if (!CanUnlock(data)) return false;

        unlocked.Add(data.id);
        OnUnlockChanged?.Invoke();

        return true;
    }
}
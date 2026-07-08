using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [SerializeField] private PlayerProfile playerProfileBase;
    private readonly List<RuntimePlayer> players = new();

    void Awake()
    {
        Instance = this;
    }

    public IReadOnlyList<RuntimePlayer> GenerateProfiles(int num)
    {
        players.Clear();

        for (int i = 0; i < num; i++)
        {
            players.Add(GetRandomPlayer());
        }

        return players;
    }

    public IEnumerable<RuntimePlayer> GetPlayerProfiles()
    {
        return players;
    }

    private RuntimePlayer GetRandomPlayer()
    {
        return new RuntimePlayer(playerProfileBase);
    }
}

public class RuntimePlayer
{
    public PlayerProfile originalProfile;

    public RuntimePlayer(PlayerProfile profile)
    {
        originalProfile = profile;
    }
}
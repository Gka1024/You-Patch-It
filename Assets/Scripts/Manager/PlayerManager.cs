using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Player Profiles")]
    [SerializeField] private List<PlayerProfile> playerProfiles = new();

    [Header("Tier Settings")]
    [SerializeField] private List<TierSetting> tierSettings = new();

    private readonly Dictionary<PlayerTier, TierSetting> settingTable = new();
    private readonly List<RuntimePlayer> players = new();

    private int totalSpawnWeight;
    private int totalProfileWeight;

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    private void Initialize()
    {
        settingTable.Clear();

        totalSpawnWeight = 0;
        totalProfileWeight = 0;

        foreach (TierSetting setting in tierSettings)
        {
            if (!settingTable.TryAdd(setting.tier, setting))
            {
                Debug.LogError($"중복된 TierSetting : {setting.tier}");
                continue;
            }

            totalSpawnWeight += setting.spawnWeight;
        }

        foreach (PlayerProfile profile in playerProfiles)
        {
            totalProfileWeight += profile.spawnWeight;
        }
    }

    //====================================================
    // Generate
    //====================================================

    public IReadOnlyList<RuntimePlayer> GeneratePlayers(int count, System.Random random)
    {
        players.Clear();

        for (int i = 0; i < count; i++)
        {
            players.Add(GenerateRandomPlayer(random));
        }

        return players;
    }

    public RuntimePlayer GenerateRandomPlayer(System.Random random)
    {
        PlayerTier tier = GetRandomTier(random);
        PlayerProfile profile = GetRandomProfile(random);

        return GeneratePlayer(tier, profile, random);
    }

    public RuntimePlayer GeneratePlayer(
        PlayerTier tier,
        PlayerProfile profile,
        System.Random random)
    {
        if (!settingTable.TryGetValue(tier, out TierSetting setting))
        {
            Debug.LogError($"TierSetting이 존재하지 않습니다. ({tier})");
            return null;
        }

        return new RuntimePlayer(profile, setting, random);
    }

    //====================================================
    // Random
    //====================================================

    private PlayerTier GetRandomTier(System.Random random)
    {
        int roll = random.Next(totalSpawnWeight);

        foreach (TierSetting setting in tierSettings)
        {
            roll -= setting.spawnWeight;

            if (roll < 0)
                return setting.tier;
        }

        return tierSettings[^1].tier;
    }

    private PlayerProfile GetRandomProfile(System.Random random)
    {
        int roll = random.Next(totalProfileWeight);

        foreach (PlayerProfile profile in playerProfiles)
        {
            roll -= profile.spawnWeight;

            if (roll < 0)
                return profile;
        }

        return playerProfiles[^1];
    }

    //====================================================
    // Getter
    //====================================================

    public IReadOnlyList<RuntimePlayer> GetPlayers()
    {
        return players;
    }
}

public enum PlayerTier
{
    Bronze,
    Silver,
    Gold,
    Platinum,
    Diamond,
    Master,
    Challenger
}
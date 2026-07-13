using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [Header("Tier Profiles")]
    [SerializeField] private List<PlayerProfile> tierProfiles = new();
    private readonly Dictionary<PlayerTier, PlayerProfile> profileTable = new();

    [Header("Tier Settings")]
    [SerializeField] private List<TierSetting> tierSettings = new();
    private readonly Dictionary<PlayerTier, TierSetting> settingTable = new();

    private readonly List<RuntimePlayer> players = new();
    private int totalWeight;

    private void Awake()
    {
        Instance = this;
        totalWeight = 0;
        Init();
    }

    private void Init()
    {
        profileTable.Clear();
        foreach (PlayerProfile profile in tierProfiles)
        {
            if (!profileTable.TryAdd(profile.tier, profile))
            {
                Debug.LogError($"중복된 PlayerProfile : {profile.tier}");
            }
        }

        settingTable.Clear();
        foreach (TierSetting setting in tierSettings)
        {
            if (!settingTable.TryAdd(setting.tier, setting))
            {
                Debug.LogError($"중복된 PlayerProfile : {setting.tier}");
            }
            totalWeight += setting.weight;
        }
    }

    public IReadOnlyList<RuntimePlayer> GeneratePlayers(int count, System.Random random)
    {
        players.Clear();

        int totalWeight = tierSettings.Sum(x => x.weight);

        for (int i = 0; i < count; i++)
        {
            PlayerTier tier = GetRandomTier(random);
            players.Add(GeneratePlayer(tier, random));
        }

        return players;
    }

    public RuntimePlayer GeneratePlayer(PlayerTier tier, System.Random random)
    {
        if (!profileTable.TryGetValue(tier, out PlayerProfile profile))
        {
            Debug.LogError($"PlayerProfile이 존재하지 않습니다. ({tier})");
            return null;
        }

        if (!settingTable.TryGetValue(tier, out TierSetting setting))
        {
            Debug.LogError($"TierSetting이 존재하지 않습니다. ({tier})");
            return null;
        }

        return new RuntimePlayer(profile, setting, random);
    }

    public IReadOnlyList<RuntimePlayer> GetPlayerProfiles()
    {
        return players;
    }

    private PlayerTier GetRandomTier(System.Random random)
    {
        int roll = random.Next(totalWeight);

        foreach (TierSetting setting in tierSettings)
        {
            roll -= setting.weight;

            if (roll < 0) return setting.tier;
        }

        return tierSettings[^1].tier;
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
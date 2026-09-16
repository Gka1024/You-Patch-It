using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private const float PlayerRetentionRate = 0.8f;
    private const int DefaultPlayerCount = 10000;

    [SerializeField] private int currentPlayerCount;
    public int CurrentPlayerCount => currentPlayerCount;
    public int GetCurrentPlayer => players.Count;

    [SerializeField] private int initialPlayerCount;

    [Header("Player Profiles")]
    [SerializeField] private List<PlayerProfile> playerProfiles = new();

    [Header("Tier Settings")]
    [SerializeField] private List<TierSetting> tierSettings = new();

    private readonly Dictionary<PlayerTier, TierSetting> settingTable = new();
    private readonly List<TierSetting> validTierSettings = new();
    private readonly List<PlayerProfile> validPlayerProfiles = new();
    private readonly List<RuntimePlayer> players = new();

    private int totalSpawnWeight;
    private int totalProfileWeight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentPlayerCount = initialPlayerCount > 0
            ? initialPlayerCount
            : DefaultPlayerCount;

        Initialize();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Initialize()
    {
        settingTable.Clear();
        validTierSettings.Clear();
        validPlayerProfiles.Clear();

        totalSpawnWeight = 0;
        totalProfileWeight = 0;

        foreach (TierSetting setting in tierSettings)
        {
            if (setting == null)
            {
                Debug.LogError("TierSetting 목록에 null 항목이 있습니다.");
                continue;
            }

            if (setting.spawnWeight <= 0)
            {
                Debug.LogWarning($"TierSetting의 가중치가 0 이하입니다. ({setting.tier})");
                continue;
            }

            if (!settingTable.TryAdd(setting.tier, setting))
            {
                Debug.LogError($"중복된 TierSetting : {setting.tier}");
                continue;
            }

            validTierSettings.Add(setting);
            totalSpawnWeight += setting.spawnWeight;
        }

        foreach (PlayerProfile profile in playerProfiles)
        {
            if (profile == null)
            {
                Debug.LogError("PlayerProfile 목록에 null 항목이 있습니다.");
                continue;
            }

            if (profile.spawnWeight <= 0)
            {
                Debug.LogWarning($"PlayerProfile의 가중치가 0 이하입니다. ({profile.name})");
                continue;
            }

            validPlayerProfiles.Add(profile);
            totalProfileWeight += profile.spawnWeight;
        }

        if (totalSpawnWeight <= 0)
            Debug.LogError("유효한 TierSetting 가중치가 없습니다.");

        if (totalProfileWeight <= 0)
            Debug.LogError("유효한 PlayerProfile 가중치가 없습니다.");
    }

    //====================================================
    // Generate / Maintain
    //====================================================

    public IReadOnlyList<RuntimePlayer> GeneratePlayers(System.Random random)
    {
        players.Clear();

        int count = Mathf.Max(0, currentPlayerCount);

        for (int i = 0; i < count; i++)
        {
            RuntimePlayer player = GenerateRandomPlayer(random);

            if (player != null)
                players.Add(player);
        }

        currentPlayerCount = players.Count;
        return players;
    }

    public IReadOnlyList<RuntimePlayer> GeneratePlayers(System.Random random, int playerCount)
    {
        List<RuntimePlayer> generatedPlayers = new();
        int count = Mathf.Max(0, playerCount);

        for (int i = 0; i < count; i++)
        {
            RuntimePlayer player = GenerateRandomPlayer(random);

            if (player != null)
                generatedPlayers.Add(player);
        }

        return generatedPlayers;
    }

    public IReadOnlyList<RuntimePlayer> UpdatePlayers(System.Random random)
    {
        // 현재 목표 인구수에 맞춰 기존 플레이어를 유지한다.
        int targetCount = Mathf.Max(0, currentPlayerCount);
        int retainedCount = Mathf.RoundToInt(players.Count * PlayerRetentionRate);
        retainedCount = Mathf.Min(retainedCount, targetCount);

        List<RuntimePlayer> shuffledPlayers = new(players);
        Shuffle(shuffledPlayers, random);

        players.Clear();

        for (int i = 0; i < retainedCount; i++)
            players.Add(shuffledPlayers[i]);

        // 잔존 인원과 목표 인구수의 차이만큼 신규 유입
        int newPlayerCount = targetCount - players.Count;

        for (int i = 0; i < newPlayerCount; i++)
        {
            RuntimePlayer player = GenerateRandomPlayer(random);

            if (player != null)
                players.Add(player);
        }

        currentPlayerCount = players.Count;

        Debug.Log($"플레이어 갱신 완료 - 기존 유지: {retainedCount}, 신규 유입: {newPlayerCount}, 전체: {players.Count}");

        return players;
    }

    public IReadOnlyList<RuntimePlayer> GetPlayers()
    {
        return players;
    }

    private void Shuffle<T>(List<T> list, System.Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    //====================================================
    // Player Creation
    //====================================================

    public RuntimePlayer GenerateRandomPlayer(System.Random random)
    {
        if (!TryGetRandomTier(random, out PlayerTier tier))
            return null;

        if (!TryGetRandomProfile(random, out PlayerProfile profile))
            return null;

        return GeneratePlayer(tier, profile, random);
    }

    public RuntimePlayer GenerateRandomPlayer(System.Random random, PlayerTier tierPreset)
    {
        if (!TryGetRandomProfile(random, out PlayerProfile profile))
            return null;

        return GeneratePlayer(tierPreset, profile, random);
    }

    public RuntimePlayer GeneratePlayer(PlayerTier tier, PlayerProfile profile, System.Random random)
    {
        if (profile == null)
        {
            Debug.LogError("PlayerProfile이 null입니다.");
            return null;
        }

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

    private bool TryGetRandomTier(System.Random random, out PlayerTier tier)
    {
        tier = default;

        if (random == null || totalSpawnWeight <= 0)
            return false;

        int roll = random.Next(totalSpawnWeight);

        foreach (TierSetting setting in validTierSettings)
        {
            roll -= setting.spawnWeight;

            if (roll < 0)
            {
                tier = setting.tier;
                return true;
            }
        }

        return false;
    }

    private bool TryGetRandomProfile(System.Random random, out PlayerProfile profile)
    {
        profile = null;

        if (random == null || totalProfileWeight <= 0)
            return false;

        int roll = random.Next(totalProfileWeight);

        foreach (PlayerProfile candidate in validPlayerProfiles)
        {
            roll -= candidate.spawnWeight;

            if (roll < 0)
            {
                profile = candidate;
                return true;
            }
        }

        return false;
    }

    //====================================================
    // Player Count
    //====================================================

    public void UpdatePlayerCount(System.Random random)
    {
        if (random == null)
        {
            Debug.LogError("UpdatePlayerCount에 전달된 Random이 null입니다.");
            return;
        }

        float trust = ResourceManager.Instance.TrustPoint;

        // 신뢰도 30을 기준으로 기본 변동률 계산
        float normalized = Mathf.Clamp((trust - 30f) / 70f, -1f, 1f);
        float baseRate = normalized * 0.2f;

        // 시즌 이슈: -4% ~ +4%
        float randomRate = (float)(random.NextDouble() * 0.08 - 0.04);

        // 기본 변동률과 시즌 이슈를 합산
        float finalRate = baseRate + randomRate;

        currentPlayerCount = Mathf.RoundToInt(currentPlayerCount * (1f + finalRate));
        currentPlayerCount = Mathf.Clamp(currentPlayerCount, 0, 1000000);

        Debug.Log($"다음 시즌 목표 인구수: {currentPlayerCount} (변동률: {finalRate:P2})");
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
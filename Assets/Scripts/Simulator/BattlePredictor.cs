using System.Collections.Generic;
using UnityEngine;

public class BattlePredictor : MonoBehaviour
{
    public static BattlePredictor Instance;

    private const int TeamSize = 3;

    [SerializeField] private int defaultSimulationCount = 100;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// baseCharacter를 레드팀에 고정했을 때의 승률을 예측한다.
    /// </summary>
    public float PredictBattle(RuntimeCharacter baseCharacter, PlayerTier tier, int simulationCount = 0)
    {
        if (baseCharacter == null)
        {
            Debug.LogError("PredictBattle: baseCharacter가 null입니다.");
            return 0f;
        }

        if (RuntimeCharacterManager.Instance == null || PlayerManager.Instance == null || BattleSimulator.Instance == null)
        {
            Debug.LogError("PredictBattle: 필요한 매니저가 초기화되지 않았습니다.");
            return 0f;
        }

        if (simulationCount <= 0)
            simulationCount = defaultSimulationCount;

        if (simulationCount <= 0)
        {
            Debug.LogError("PredictBattle: 시뮬레이션 횟수가 0 이하입니다.");
            return 0f;
        }

        System.Random random = new System.Random();

        // 캐릭터 후보 확보
        List<RuntimeCharacter> availableCharacters = new(RuntimeCharacterManager.Instance.GetAllCharacters());

        availableCharacters.RemoveAll(character => character == null);

        // 레드팀은 기준 캐릭터를 제외하고 최소 2명 필요
        List<RuntimeCharacter> redCandidates = new(availableCharacters);
        redCandidates.Remove(baseCharacter);

        // 블루팀은 최소 3명 필요
        if (redCandidates.Count < TeamSize - 1 || availableCharacters.Count < TeamSize)
        {
            Debug.LogError("PredictBattle: 팀을 구성하기 위한 캐릭터가 부족합니다.");
            return 0f;
        }

        // 예측용 플레이어 6명 생성
        List<RuntimePlayer> players = new();

        for (int i = 0; i < TeamSize * 2; i++)
        {
            RuntimePlayer player = PlayerManager.Instance.GenerateRandomPlayer(random, tier);

            if (player == null)
            {
                Debug.LogError("PredictBattle: 예측용 플레이어 생성에 실패했습니다.");
                return 0f;
            }

            players.Add(player);
        }

        List<RuntimePlayer> redPlayers = players.GetRange(0, TeamSize);
        List<RuntimePlayer> bluePlayers = players.GetRange(TeamSize, TeamSize);

        int redWins = 0;

        // 시뮬레이션 반복
        for (int i = 0; i < simulationCount; i++)
        {
            // 레드팀: 기준 캐릭터 + 무작위 2명
            List<RuntimeCharacter> redCharacters = new()
            {
                baseCharacter
            };

            redCharacters.AddRange(SelectRandomCharacters(redCandidates, TeamSize - 1, random));

            // 블루팀: 무작위 3명
            // 레드팀과의 캐릭터 중복은 허용
            List<RuntimeCharacter> blueCharacters = SelectRandomCharacters(availableCharacters, TeamSize, random);

            // 실제 전투 시뮬레이션
            int battleSeed = random.Next();

            BattleResult result = BattleSimulator.Instance.Simulate(
                redCharacters,
                redPlayers,
                blueCharacters,
                bluePlayers,
                battleSeed
            );

            if (result == null)
                continue;

            if (result.isRedWin)
                redWins++;
        }

        return (float)redWins / simulationCount * 100f;
    }

    /// <summary>
    /// redCharacter를 레드팀, blueCharacter를 블루팀에 고정했을 때
    /// 레드팀의 승률을 예측한다.
    /// </summary>
    public float PredictBattle(RuntimeCharacter redCharacter, RuntimeCharacter blueCharacter, PlayerTier tier, int simulationCount = 0)
    {
        if (redCharacter == null || blueCharacter == null)
        {
            Debug.LogError("PredictBattle: 캐릭터가 null입니다.");
            return 0f;
        }

        if (RuntimeCharacterManager.Instance == null || PlayerManager.Instance == null || BattleSimulator.Instance == null)
        {
            Debug.LogError("PredictBattle: 필요한 매니저가 초기화되지 않았습니다.");
            return 0f;
        }

        if (simulationCount <= 0)
            simulationCount = defaultSimulationCount;

        if (simulationCount <= 0)
        {
            Debug.LogError("PredictBattle: 시뮬레이션 횟수가 0 이하입니다.");
            return 0f;
        }

        System.Random random = new System.Random();

        List<RuntimeCharacter> availableCharacters = new(RuntimeCharacterManager.Instance.GetAllCharacters());
        availableCharacters.RemoveAll(character => character == null);

        // 각 팀에서 고정 캐릭터를 제외한 후보 확보
        List<RuntimeCharacter> redCandidates = new(availableCharacters);
        List<RuntimeCharacter> blueCandidates = new(availableCharacters);

        redCandidates.Remove(redCharacter);
        blueCandidates.Remove(blueCharacter);

        if (redCandidates.Count < TeamSize - 1 || blueCandidates.Count < TeamSize - 1)
        {
            Debug.LogError("PredictBattle: 팀을 구성하기 위한 캐릭터가 부족합니다.");
            return 0f;
        }

        // 예측용 플레이어 생성
        List<RuntimePlayer> players = new();

        for (int i = 0; i < TeamSize * 2; i++)
        {
            RuntimePlayer player = PlayerManager.Instance.GenerateRandomPlayer(random, tier);

            if (player == null)
            {
                Debug.LogError("PredictBattle: 예측용 플레이어 생성에 실패했습니다.");
                return 0f;
            }

            players.Add(player);
        }

        List<RuntimePlayer> redPlayers = players.GetRange(0, TeamSize);
        List<RuntimePlayer> bluePlayers = players.GetRange(TeamSize, TeamSize);

        int redWins = 0;

        for (int i = 0; i < simulationCount; i++)
        {
            // 레드팀: A 캐릭터 + 랜덤 2명
            List<RuntimeCharacter> redCharacters = new()
        {
            redCharacter
        };

            redCharacters.AddRange(SelectRandomCharacters(redCandidates, TeamSize - 1, random));

            // 블루팀: B 캐릭터 + 랜덤 2명
            List<RuntimeCharacter> blueCharacters = new()
        {
            blueCharacter
        };

            blueCharacters.AddRange(SelectRandomCharacters(blueCandidates, TeamSize - 1, random));

            int battleSeed = random.Next();

            BattleResult result = BattleSimulator.Instance.Simulate(
                redCharacters,
                redPlayers,
                blueCharacters,
                bluePlayers,
                battleSeed
            );

            if (result == null)
                continue;

            if (result.isRedWin)
                redWins++;
        }

        return (float)redWins / simulationCount * 100f;
    }

    /// <summary>
    /// 중복 없이 지정된 수만큼 무작위 캐릭터를 선택한다.
    /// </summary>
    private List<RuntimeCharacter> SelectRandomCharacters(List<RuntimeCharacter> candidates, int count, System.Random random)
    {
        List<RuntimeCharacter> pool = new(candidates);

        // 부분 Fisher-Yates Shuffle
        for (int i = 0; i < count; i++)
        {
            int randomIndex = random.Next(i, pool.Count);

            (pool[i], pool[randomIndex]) = (pool[randomIndex], pool[i]);
        }

        return pool.GetRange(0, count);
    }
}
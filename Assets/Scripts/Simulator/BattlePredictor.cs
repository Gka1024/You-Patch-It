using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattlePredictor : MonoBehaviour
{
    public static BattlePredictor Instance;

    [SerializeField] private int defaultSimulationCount = 100;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// baseCharacter가 opponentCharacter를 이길 확률(0~100)
    /// </summary>
    public float PredictBattle(RuntimeCharacter baseCharacter, RuntimeCharacter opponentCharacter, int simulationCount = -1)
    {
        if (simulationCount <= 0) simulationCount = defaultSimulationCount;

        int winCount = 0;

        // 예측용 Random
        System.Random random = new System.Random(Guid.NewGuid().GetHashCode());

        for (int i = 0; i < simulationCount; i++)
        {
            RuntimePlayer redPlayer = PlayerManager.Instance.GenerateRandomPlayer(random);
            RuntimePlayer bluePlayer = PlayerManager.Instance.GenerateRandomPlayer(random);

            BattleResult result = BattleSimulator.Instance.Simulate(baseCharacter, redPlayer, opponentCharacter, bluePlayer, random.Next());

            if (result.winner[0] == baseCharacter)
                winCount++;
        }

        return (float)winCount / simulationCount * 100f;
    }
}
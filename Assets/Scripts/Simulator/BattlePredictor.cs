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
        return 0;
    }
}
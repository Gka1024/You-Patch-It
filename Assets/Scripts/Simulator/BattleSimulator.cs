using UnityEngine;

public class BattleSimulator : MonoBehaviour
{
    public static BattleSimulator Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void StartSimulation()
    {
        Debug.Log("Simulation");

        // 전투 계산

        SeasonManager.Instance.FinishSimulation();
    }

    
}
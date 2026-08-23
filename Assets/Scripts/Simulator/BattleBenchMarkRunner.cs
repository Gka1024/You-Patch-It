using UnityEngine;

public class BattleBenchmarkRunner : MonoBehaviour
{
    [SerializeField] private int simulationCount = 5000;

    [ContextMenu("Run Battle Benchmark")]
    public void Run()
    {
        BattleSimulationBenchmark benchmark = new BattleSimulationBenchmark(BattleSimulator.Instance);

        benchmark.RunBenchmark(simulationCount);
    }
}
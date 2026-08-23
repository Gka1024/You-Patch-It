using System;
//using System.Diagnostics;
using UnityEngine;

public class BattleSimulationBenchmark
{
    private readonly BattleSimulator simulator;

    public BattleSimulationBenchmark(BattleSimulator simulator)
    {
        this.simulator = simulator;
    }

    public BattleBenchmarkResult RunBenchmark(int simulationCount = 5000)
    {
        if (simulator == null)
        {
            Debug.LogError("[Battle Benchmark] BattleSimulator가 없습니다.");
            return null;
        }

        RuntimeCharacter redCharacter = RuntimeCharacterManager.Instance.GetRandomCharacter();
        RuntimeCharacter blueCharacter = RuntimeCharacterManager.Instance.GetRandomCharacter();

        if (redCharacter == null || blueCharacter == null)
        {
            Debug.LogError("[Battle Benchmark] RuntimeCharacter를 찾을 수 없습니다.");
            return null;
        }

        System.Random random = new(Guid.NewGuid().GetHashCode());

        RuntimePlayer redPlayer = PlayerManager.Instance.GenerateRandomPlayer(random);
        RuntimePlayer bluePlayer = PlayerManager.Instance.GenerateRandomPlayer(random);

        PrepareMeasurement();

        long heapBefore = GC.GetTotalMemory(false);

        int GCBefore = GC.CollectionCount(0);

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < simulationCount; i++)
        {
            int battleSeed = random.Next();

            simulator.Simulate(
                redCharacter,
                redPlayer,
                blueCharacter,
                bluePlayer,
                battleSeed
            );
        }

        stopwatch.Stop();

        long heapAfter = GC.GetTotalMemory(false);

        int GCAfter = GC.CollectionCount(0);

        BattleBenchmarkResult result = new BattleBenchmarkResult(
            simulationCount,
            stopwatch.Elapsed.TotalMilliseconds,
            heapBefore,
            heapAfter,
            GCAfter - GCBefore,
            simulator.CommandPoolCount,
            simulator.CharacterPoolCount
        );

        result.Log();

        return result;
    }

    private static void PrepareMeasurement()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}

public class BattleBenchmarkResult
{
    public int SimulationCount { get; }

    public double TotalMilliseconds { get; }
    public double AverageMilliseconds { get; }

    public long HeapBefore { get; }
    public long HeapAfter { get; }
    public long HeapDelta => HeapAfter - HeapBefore;

    public int GCCollections { get; }

    public int CommandPoolCount { get; }
    public int CharacterPoolCount { get; }

    public BattleBenchmarkResult(
        int simulationCount,
        double totalMilliseconds,
        long heapBefore,
        long heapAfter,
        int gcCollections,
        int commandPoolCount,
        int characterPoolCount)
    {
        SimulationCount = simulationCount;

        TotalMilliseconds = totalMilliseconds;
        AverageMilliseconds = totalMilliseconds / simulationCount;

        HeapBefore = heapBefore;
        HeapAfter = heapAfter;

        GCCollections = gcCollections;

        CommandPoolCount = commandPoolCount;
        CharacterPoolCount = characterPoolCount;
    }

    public void Log()
    {
        Debug.Log(
            "========================================\n" +
            "       BATTLE SIMULATION BENCHMARK\n" +
            "========================================\n" +
            $"Simulation Count : {SimulationCount:N0}\n" +
            $"Total Time       : {TotalMilliseconds:N2} ms\n" +
            $"Average Time     : {AverageMilliseconds:F4} ms / battle\n" +
            $"GC Heap Before   : {FormatMemory(HeapBefore)}\n" +
            $"GC Heap After    : {FormatMemory(HeapAfter)}\n" +
            $"GC Heap Delta    : {FormatMemory(HeapDelta)}\n" +
            $"GC Count          : {GCCollections:N0}\n" +
            $"Command Pool     : {CommandPoolCount:N0}\n" +
            $"Character Pool   : {CharacterPoolCount:N0}\n" +
            "========================================"
        );
    }

    private static string FormatMemory(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";

        if (bytes < 1024 * 1024)
            return $"{bytes / 1024f:F2} KB";

        return $"{bytes / (1024f * 1024f):F2} MB";
    }
}
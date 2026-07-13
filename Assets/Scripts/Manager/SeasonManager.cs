using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public enum SeasonState
{
    None,
    Start,
    Patch,
    GeneratePlayer,
    Pick,
    Simulation,
    Result,
    Trust,
    Reward,
    End
}

public class SeasonManager : MonoBehaviour
{
    public static SeasonManager Instance;

    public int CurrentSeason { get; private set; } = 1;
    public SeasonState CurrentState { get; private set; }

    public int SeasonSeed { get; private set; }
    private System.Random SeasonRandom;

    List<RuntimePlayer> players;
    List<MatchData> matches;
    List<BattleResult> results;

    private void Awake()
    {
        if (Instance != null)
        {
            return;
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        StartSeason();
    }

    public int NextSeed()
    {
        return SeasonRandom.Next();
    }

    public void StartSeason()
    {
        Debug.Log($"============  Season {CurrentSeason}  ==============");

        SeasonSeed = UnityEngine.Random.Range(0, int.MaxValue);
        SeasonRandom = new System.Random(SeasonSeed);
        Debug.Log($"Season : {CurrentSeason} || Seed : {SeasonSeed}");

        ChangeState(SeasonState.Start);
        ChangeState(SeasonState.Patch);
    }

    public void FinishPatch()
    {
        ChangeState(SeasonState.GeneratePlayer);
    }

    public void FinishGeneratePlayer()
    {
        ChangeState(SeasonState.Pick);
    }

    public void FinishPick()
    {
        ChangeState(SeasonState.Simulation);
    }

    public void FinishSimulation()
    {
        ChangeState(SeasonState.Result);
    }

    public void FinishResult()
    {
        ChangeState(SeasonState.Reward);
    }

    public void FinishReward()
    {
        ChangeState(SeasonState.End);
    }

    public void NextSeason()
    {
        CurrentSeason++;
        StartSeason();
    }

    private void ChangeState(SeasonState state)
    {
        CurrentState = state;

        Debug.Log($"State : {state}");

        switch (state)
        {
            case SeasonState.Start:
                break;

            case SeasonState.Patch:
                PatchManager.Instance.StartPatch();
                break;

            case SeasonState.GeneratePlayer:
                players = PlayerManager.Instance.GeneratePlayers(10000, SeasonRandom).ToList();
                FinishGeneratePlayer();
                break;

            case SeasonState.Pick:
                matches = PickManager.Instance.StartPick(players, SeasonRandom);
                FinishPick();
                break;

            case SeasonState.Simulation:
                StatisticsManager.Instance.ResetSeason();
                results = BattleSimulator.Instance.StartSimulation(matches, SeasonRandom);
                FinishSimulation();
                break;

            case SeasonState.Result:
                StatisticsManager.Instance.RecordBattle(results);
                ResultManager.Instance.GenerateResult();
                break;

            case SeasonState.Trust:
                TrustManager.Instance.CalculateTrust();
                break;

            case SeasonState.Reward:
                ResourceManager.Instance.GiveReward();
                break;

            case SeasonState.End:
                NextSeason();
                break;
        }
    }

}
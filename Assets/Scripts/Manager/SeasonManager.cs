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
    public int CurrentSubSeason { get; private set; } = 1;
    public SeasonState CurrentState { get; private set; }

    public int SeasonSeed { get; private set; }
    private System.Random SeasonRandom;

    public UpDisplayUI upDisplayUI;

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
        CurrentSubSeason = 1;
        StartSubSeason();
        ChangeState(SeasonState.Start);
    }

    private void StartSubSeason()
    {
        Debug.Log($"============  Season {CurrentSeason} - {CurrentSubSeason} ==============");

        SeasonSeed = UnityEngine.Random.Range(0, int.MaxValue);
        SeasonRandom = new System.Random(SeasonSeed);
        Debug.Log($"Season : {CurrentSeason} - {CurrentSubSeason} || Seed : {SeasonSeed}");
        ChangeState(SeasonState.Patch);
    }

    public void FinishStart()
    {
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
        NextSubSeason();
    }

    public void FinishTrust()
    {
        ChangeState(SeasonState.Reward);
    }

    private void NextSubSeason()
    {
        CurrentSubSeason++;

        if (CurrentSubSeason > 3)
        {
            ChangeState(SeasonState.Trust);
        }
        else
        {
            StartSubSeason();
        }
    }

    public void FinishReward()
    {
        ChangeState(SeasonState.End);
    }

    public void NextSeason()
    {
        CurrentSeason++;
        CurrentSubSeason = 1;
        StartSeason();
    }

    private void ChangeState(SeasonState state)
    {
        CurrentState = state;

        Debug.Log($"State : {state}");

        switch (state)
        {
            case SeasonState.Start:
                UIManager.Instance.dashBoardUI.ShowGoals();
                GoalManager.Instance.ResetRerollCount();
                break;

            case SeasonState.Patch:
                UIManager.Instance.dashBoardUI.ShowCharacter();
                upDisplayUI.Refresh();
                PatchManager.Instance.StartPatch();
                break;

            case SeasonState.GeneratePlayer:
                players = PlayerManager.Instance.GeneratePlayers(50000, SeasonRandom).ToList();
                FinishGeneratePlayer();
                break;

            case SeasonState.Pick:
                matches = PickManager.Instance.StartPick(players, SeasonRandom);
                FinishPick();
                break;

            case SeasonState.Simulation:
                StatisticsManager.Instance.ResetSeason(true);
                results = BattleSimulator.Instance.StartSimulation(matches, SeasonRandom);
                FinishSimulation();
                break;

            case SeasonState.Result:
                StatisticsManager.Instance.RecordBattle(results);
                AnalysisManager.Instance.AnalyzeSeason();
                ResultManager.Instance.GenerateResult();
                GoalManager.Instance.EvaluateAllGoals();
                break;

            case SeasonState.Trust:
                GoalManager.Instance.CalculateGoals();
                TrustManager.Instance.CalculateTrust();
                FinishTrust();
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
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

    public int DisplaySeason { get; private set; } = 1;
    public int DisplaySubSeason { get; private set; } = 1;

    public int SeasonSeed { get; private set; }
    private System.Random SeasonRandom;

    public UpDisplayUI upDisplayUI;
    public CharacterTableUI characterTableUI;
    public PatchNoteUI patchNoteUI;

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
        SeasonSeed = UnityEngine.Random.Range(0, int.MaxValue);
        SeasonRandom = new System.Random(SeasonSeed);

        Debug.Log($"Season : {DisplaySeason}-{DisplaySubSeason} || Seed : {SeasonSeed}");

        ChangeState(SeasonState.Patch);
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

    public void NextSeason()
    {
        CurrentSeason++;
        CurrentSubSeason = 1;

        StartSeason();
    }

    public void FinishStart()
    {
        ChangeState(SeasonState.Patch);
    }

    public void FinishPatch()
    {
        DisplaySeason = CurrentSeason;
        DisplaySubSeason = CurrentSubSeason;

        upDisplayUI.Refresh();

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

    public void FinishReward()
    {
        ChangeState(SeasonState.End);
    }

    private void ChangeState(SeasonState state)
    {
        CurrentState = state;

        Debug.Log($"State : {state}");

        switch (state)
        {
            case SeasonState.Start: // 신규 캐릭터 추가, 리롤 횟수 초기화
                GoalManager.Instance.SeasonReset();
                break;

            case SeasonState.Patch:
                UIManager.Instance.dashBoardUI.ShowCharacter();
                upDisplayUI.Refresh();
                PatchManager.Instance.StartPatch();
                break;

            case SeasonState.GeneratePlayer:
                players = PlayerManager.Instance.GeneratePlayers(SeasonRandom).ToList();
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
                PatchHistoryManager.Instance.SaveCurrentSeason();
                break;

            case SeasonState.Trust:
                GoalManager.Instance.CalculateGoals();
                TrustManager.Instance.CalculateTrust();
                FinishTrust();
                break;

            case SeasonState.Reward:
                ResourceManager.Instance.CheckReward();
                break;

            case SeasonState.End:
                if (RuntimeCharacterManager.Instance.HasLockedCharacter())
                {
                    RuntimeCharacterManager.Instance.AddRandomCharacter(SeasonRandom);
                    patchNoteUI.InitializeDropdown();
                    characterTableUI.GenerateTable();
                }
                PlayerManager.Instance.UpdatePlayerCount(SeasonRandom);
                NextSeason();
                break;
        }
    }

}
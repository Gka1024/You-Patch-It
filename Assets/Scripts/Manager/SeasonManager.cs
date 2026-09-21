using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public static SeasonManager Instance { get; private set; }

    private const int SubSeasonCount = 3;

    public int CurrentSeason { get; private set; } = 1;
    public int CurrentSubSeason { get; private set; } = 1;
    public SeasonState CurrentState { get; private set; }

    public int DisplaySeason { get; private set; } = 1;
    public int DisplaySubSeason { get; private set; } = 1;

    public int SeasonSeed { get; private set; }
    private System.Random seasonRandom;

    private bool isSeasonFinished;
    public bool IsSeasonFinished => isSeasonFinished;

    public event System.Action OnSeasonEnd;

    private List<RuntimePlayer> players = new();
    private List<MatchData> matches = new();
    private List<BattleResult> results = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartSeason();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public int NextSeed()
    {
        return seasonRandom.Next();
    }

    //====================================================
    // Season Flow
    //====================================================

    public void StartSeason()
    {
        CurrentSubSeason = 1;
        ChangeState(SeasonState.Start);
    }

    private void StartSubSeason()
    {
        SeasonSeed = UnityEngine.Random.Range(0, int.MaxValue);
        seasonRandom = new System.Random(SeasonSeed);

        Debug.Log($"Season : {CurrentSeason}-{CurrentSubSeason} || Seed : {SeasonSeed}");

        ChangeState(SeasonState.Patch);
    }

    private void NextSubSeason()
    {
        CurrentSubSeason++;

        if (CurrentSubSeason > SubSeasonCount)
        {
            ChangeState(SeasonState.Trust);
            return;
        }

        StartSubSeason();
    }

    public void NextSeason()
    {
        if (!isSeasonFinished)
            return;

        isSeasonFinished = false;
        CurrentSeason++;
        CurrentSubSeason = 1;

        StartSeason();
    }

    public void CheckSeasonFinished()
    {
        if (isSeasonFinished)
            NextSeason();
    }

    //====================================================
    // State Finish
    //====================================================

    public void FinishStart()
    {
        StartSubSeason();
    }

    public void FinishPatch()
    {
        DisplaySeason = CurrentSeason;
        DisplaySubSeason = CurrentSubSeason;

        UIManager.Instance.upDisplayUI.Refresh();

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

    //====================================================
    // State Machine
    //====================================================

    private void ChangeState(SeasonState state)
    {
        CurrentState = state;

        Debug.Log($"State : {state}");

        switch (state)
        {
            case SeasonState.Start:
                GoalManager.Instance.SeasonReset();
                FinishStart();
                break;

            case SeasonState.Patch:
                UIManager.Instance.dashBoardUI.ShowCharacter();
                UIManager.Instance.upDisplayUI.Refresh();
                PatchManager.Instance.StartPatch();
                break;

            case SeasonState.GeneratePlayer:
                GeneratePlayersForCurrentSubSeason();
                FinishGeneratePlayer();
                break;

            case SeasonState.Pick:
                matches = PickManager.Instance.StartPick(players, seasonRandom);
                FinishPick();
                break;

            case SeasonState.Simulation:
                StatisticsManager.Instance.ResetSeason();
                results = BattleSimulator.Instance.StartSimulation(matches, seasonRandom);
                FinishSimulation();
                break;

            case SeasonState.Result:
                StatisticsManager.Instance.RecordBattle(results);
                StatisticsManager.Instance.SaveCurrentSubSeason(CurrentSeason, CurrentSubSeason);
                AnalysisManager.Instance.AnalyzeSeason();
                ResultManager.Instance.GenerateResult(false);
                GoalManager.Instance.EvaluateAllGoals();
                PatchHistoryManager.Instance.SaveCurrentSeason();
                UIManager.Instance.patchNoteUI.Refresh();
                UIManager.Instance.characterTableUI.ReArrangetable();
                RuntimeCharacterManager.Instance.ResetAllCharacter();

                if (!UIManager.Instance.bottomDisplayUI.UserReaction.LoopOn)
                    UIManager.Instance.bottomDisplayUI.UserReaction.TurnOnLoop();

                UIManager.Instance.bottomDisplayUI.ShowReaction();
                break;

            case SeasonState.Trust:
                ResourceManager.Instance.ResetCurrentSeason();
                GoalManager.Instance.CalculateGoals();
                ResourceManager.Instance.CalculateSeasonReward();
                FinishTrust();
                break;

            case SeasonState.Reward:
                ResourceManager.Instance.CheckGameOver();
                RuntimeCharacterManager.Instance.AddRandomCharacter(seasonRandom);
                UIManager.Instance.patchNoteUI.InitializeDropdown();
                UIManager.Instance.characterTableUI.GenerateTable();
                FinishReward();
                break;

            case SeasonState.End:
                isSeasonFinished = true;

                PlayerManager.Instance.UpdatePlayerCount(seasonRandom);

                OnSeasonEnd?.Invoke();

                GoalManager.Instance.ChangeGoals();
                UIManager.Instance.dashBoardUI.ShowSeasonReports();
                UIManager.Instance.seasonReportUI.Initialize(CurrentSeason);
                UIManager.Instance.bottomDisplayUI.GoalPreview.Reset();
                break;
        }
    }

    private void GeneratePlayersForCurrentSubSeason()
    {
        PlayerManager playerManager = PlayerManager.Instance;

        if (CurrentSeason == 1 && CurrentSubSeason == 1)
        {
            // 게임 최초 시작: 전체 플레이어 생성
            players = playerManager.GeneratePlayers(seasonRandom).ToList();
        }
        else if (CurrentSubSeason == 1)
        {
            // 새 시즌 첫 서브시즌: 잔존 플레이어 + 신규 유입
            players = playerManager.UpdatePlayers(seasonRandom).ToList();
        }
        else
        {
            // 같은 시즌의 다음 서브시즌: 기존 플레이어 유지
            players = playerManager.GetPlayers().ToList();
        }
    }
}
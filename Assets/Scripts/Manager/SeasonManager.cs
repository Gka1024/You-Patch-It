using UnityEngine;
using UnityEngine.XR;

public enum SeasonState
{
    None,
    Start,
    Patch,
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

    public void StartSeason()
    {
        Debug.Log($"============  Season {CurrentSeason}  ==============");

        ChangeState(SeasonState.Start);
        ChangeState(SeasonState.Patch);
    }

    public void FinishPatch()
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

            case SeasonState.Simulation:
                StatisticsManager.Instance.ResetSeason();
                BattleSimulator.Instance.StartSimulation();
                break;

            case SeasonState.Result:
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
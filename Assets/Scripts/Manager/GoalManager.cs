using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance;
    public CharacterDatabase characterDatabase;

    public DeveloperGoalUI GoalUI;

    private List<DeveloperGoal> GoalList = new();
    private List<DeveloperGoal> shuffledGoals = new();
    private Dictionary<GoalDifficulty, GoalReward> RewardTable = new();

    private int rerollCount;
    private const int REROLL_REQUIRE_RESOURCE = 10;

    private bool isRerollAvailable;
    private bool isGoalConfirmed;
    public bool IsGoalSet => isGoalConfirmed;

    private const int GOAL_REWARD = 2011;
    private const int ADDITIONAL_SLOT = 2021;
    private const int FREE_REROLL = 2031;

    private const int ADDITIONAL_GOAL_I = 2041;
    private const int ADDITIONAL_GOAL_II = 2042;
    private const int ADDITIONAL_GOAL_III = 2043;

    void Awake()
    {
        Instance = this;
        GenerateRewards();
        ResetRerollCount();
    }

    void Start()
    {
        GenerateGoals();
        SetGoals();
        GoalUI.Initialize(shuffledGoals, this);
        GoalUI.SetRerollCostValue(REROLL_REQUIRE_RESOURCE * rerollCount);
        UnlockManager.Instance.OnUnlockChanged += CheckThirdGoal;
        UnlockManager.Instance.OnUnlockChanged += CheckGoalsTier1;
        UnlockManager.Instance.OnUnlockChanged += CheckGoalsTier2;
        UnlockManager.Instance.OnUnlockChanged += CheckGoalsTier3;
    }

    public void GenerateGoals()
    {
        GoalList.Clear();

        GoalList.Add(new BottomToTopGoal(AnalysisManager.Instance.GetLowestCharacter(AnalysisItem.Winrate, true), 3, GoalDifficulty.Normal, GoalType.Balance));
        GoalList.Add(new VeteranMakerGoal(57f, GoalDifficulty.Normal, GoalType.Balance));
        GoalList.Add(new WinrateBandGoal(49f, 54f, 4, GoalDifficulty.Hard, GoalType.Balance));
        GoalList.Add(new WinrateRangeGoal(45f, 55f, GoalDifficulty.Impossible, GoalType.Balance));

        GoalList.Add(new PredictCharacterWinrateRank(RuntimeCharacterManager.Instance.GetRandomCharacter().OriginCharacter, Random.Range(1, 9), GoalDifficulty.Impossible, GoalType.Challenge));
        GoalList.Add(new SpecificCharacterWinrateGoal(40, 60, RuntimeCharacterManager.Instance.GetRandomCharacter().OriginCharacter, GoalDifficulty.Hard, GoalType.Challenge));

        GoalList.Add(new MaxPickRateGoal(12f, GoalDifficulty.Hard, GoalType.Meta));
        GoalList.Add(new MinPickRateGoal(4f, GoalDifficulty.Hard, GoalType.Meta));
        GoalList.Add(new ReverseMetaGoal(GoalDifficulty.Hard, GoalType.Meta));
        GoalList.Add(new RolePickrateGoal(6f, GoalDifficulty.Easy, GoalType.Meta));

        GoalList.Add(new MobilityPatchGoal(GoalDifficulty.Easy, GoalType.Patch));
        GoalList.Add(new NoAttackPatchGoal(GoalDifficulty.Easy, GoalType.Patch));
        GoalList.Add(new PatchCountGoal(3, GoalDifficulty.Normal, GoalType.Patch));
        GoalList.Add(new PrecisionPatchGoal(GoalDifficulty.Normal, GoalType.Patch));
        GoalList.Add(new SingleStatPatchGoal(GoalDifficulty.Hard, GoalType.Patch));
    }

    private void CheckGoalsTier1()
    {
        if (UnlockManager.Instance.IsUnlocked(ADDITIONAL_GOAL_I))
        {
            UnlockManager.Instance.OnUnlockChanged -= CheckGoalsTier1;

        }
    }

    private void CheckGoalsTier2()
    {
        if (UnlockManager.Instance.IsUnlocked(ADDITIONAL_GOAL_II))
        {
            UnlockManager.Instance.OnUnlockChanged -= CheckGoalsTier2;

        }
    }

    private void CheckGoalsTier3()
    {
        if (UnlockManager.Instance.IsUnlocked(ADDITIONAL_GOAL_III))
        {
            UnlockManager.Instance.OnUnlockChanged -= CheckGoalsTier3;

        }
    }

    public void SetGoals()
    {
        shuffledGoals.Clear();

        int goalCount = 2;
        if (UnlockManager.Instance != null)
        {
            goalCount = UnlockManager.Instance.IsUnlocked(ADDITIONAL_SLOT) ? 3 : 2;
        }

        ShuffleGoals();

        shuffledGoals = GetGoals(goalCount);

        GoalUI.SetGoals(shuffledGoals);
    }

    private void ShuffleGoals()
    {
        shuffledGoals.Clear();
        List<DeveloperGoal> shuffled = new(GoalList);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        for (int i = 0; i < shuffled.Count; i++)
        {
            shuffledGoals.Add(shuffled[i]);
        }
    }

    private List<DeveloperGoal> GetGoals(int count)
    {
        List<DeveloperGoal> result = new();

        for (int i = 0; i < count; i++)
        {
            result.Add(shuffledGoals[i]);
        }

        return result;
    }

    public void ChangeGoals()
    {
        if (!isRerollAvailable) return;
        if (!ResourceManager.Instance.SpendDevelopResource(REROLL_REQUIRE_RESOURCE * rerollCount)) return;

        GoalUI.SetRerollCostValue(REROLL_REQUIRE_RESOURCE * ++rerollCount);
        SetGoals();
    }

    private void CheckThirdGoal()
    {
        if (UnlockManager.Instance.IsUnlocked(ADDITIONAL_SLOT))
        {
            UnlockManager.Instance.OnUnlockChanged -= CheckThirdGoal;

            if (shuffledGoals.Count < 3) ShuffleGoals();
            GoalUI.SetGoals(shuffledGoals[2], 2);
        }
    }

    public void ConfirmGoals()
    {
        GoalUI.ShowAlert(false);
        isGoalConfirmed = true;
        isRerollAvailable = false;
        SeasonManager.Instance.FinishStart();
    }

    public void SeasonReset()
    {
        GoalUI.ShowAlert(true);
        ResetRerollCount();
        GenerateGoals();
        SetGoals();
    }

    public void ResetRerollCount()
    {
        isRerollAvailable = true;
        isGoalConfirmed = false;

        if (UnlockManager.Instance == null)
        {
            rerollCount = 1;
        }
        else
        {
            rerollCount = UnlockManager.Instance.IsUnlocked(FREE_REROLL) ? 0 : 1;
        }

        GoalUI.SetRerollCostValue(REROLL_REQUIRE_RESOURCE * rerollCount);
    }

    private void GenerateRewards()
    {
        RewardTable.Add(GoalDifficulty.Easy, new GoalReward(100, 25));
        RewardTable.Add(GoalDifficulty.Normal, new GoalReward(150, 35));
        RewardTable.Add(GoalDifficulty.Hard, new GoalReward(300, 50));
        RewardTable.Add(GoalDifficulty.Impossible, new GoalReward(500, 75));
    }

    public GoalReward GetReward(GoalDifficulty difficulty)
    {
        RewardTable.TryGetValue(difficulty, out GoalReward reward);

        reward.DevelopResource *= (int)(UnlockManager.Instance.IsUnlocked(GOAL_REWARD) ? 1.2f : 1f);
        reward.TrustPoint *= (int)(UnlockManager.Instance.IsUnlocked(GOAL_REWARD) ? 1.2f : 1f);

        return reward;
    }

    public void EvaluateAllGoals()
    {
        foreach (DeveloperGoal goal in shuffledGoals)
        {
            goal.Evaluate();
        }

        RefreshUI();
        //todo ui에 진행상황 연결하기
    }

    public void CalculateGoals()
    {
        Debug.Log("CalculateGoals");
        EvaluateAllGoals();

        foreach (DeveloperGoal goal in shuffledGoals)
        {
            if (goal.IsComplete)
            {
                ResourceManager.Instance.AddReward(goal.Reward);
            }
        }
    }

    private void RefreshUI()
    {
        GoalUI.RefreshUI();
    }

}

public enum GoalDifficulty
{
    Easy,
    Normal,
    Hard,
    Impossible
}
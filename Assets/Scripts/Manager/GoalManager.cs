using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance;

    public CharacterDatabase characterDatabase;
    public DeveloperGoalUI GoalUI;
    public BottomGoalPreviewUI BottomGoalUI;

    private List<DeveloperGoal> GoalList = new();
    private List<DeveloperGoal> shuffledGoals = new();
    private Dictionary<GoalDifficulty, GoalReward> RewardTable = new();

    public bool IsGoalAvailable;
    private int currentGoalCount;
    private int rerollCount;

    private const int REROLL_REQUIRE_RESOURCE = 10;

    private bool isRerollAvailable;
    private bool isGoalConfirmed;

    public bool IsGoalSet => isGoalConfirmed;

    public event System.Action OnGoalChanged;
    public event System.Action OnGoalConfirmed;

    private const int GOAL_REWARD = 2011;
    private const int ADDITIONAL_SLOT_1 = 2021;
    private const int ADDITIONAL_SLOT_2 = 2022;
    private const int FREE_REROLL = 2031;

    private const int ADDITIONAL_GOAL_I = 2041;
    private const int ADDITIONAL_GOAL_II = 2042;
    private const int ADDITIONAL_GOAL_III = 2043;

    private void Awake()
    {
        Instance = this;

        IsGoalAvailable = false;
        currentGoalCount = 1;

        GenerateRewards();
        ResetRerollCount();
    }

    private void Start()
    {
        GenerateGoals();
        SetGoals();

        GoalUI.Initialize(shuffledGoals, this);
        BottomGoalUI.Initialize(this);
        GoalUI.SetRerollCostValue(REROLL_REQUIRE_RESOURCE * rerollCount);

        UnlockManager.Instance.OnUnlockChanged += CheckSecondGoal;
        UnlockManager.Instance.OnUnlockChanged += CheckThirdGoal;

        UnlockManager.Instance.OnUnlockChanged += AddGoalsTier1;
        UnlockManager.Instance.OnUnlockChanged += AddGoalsTier2;
        UnlockManager.Instance.OnUnlockChanged += AddGoalsTier3;
    }

    //=========================================================
    // Goal
    //=========================================================

    public void GenerateGoals()
    {
        GoalList.Clear();

        GoalList.Add(new NerfTopGoal(GoalDifficulty.Easy, GoalType.Challenge));

        GoalList.Add(new SpecificCharacterWinrateGoal(40, 60, GoalDifficulty.Easy, GoalType.Challenge));
    }

    private void AddGoalsTier1()
    {
        if (!UnlockManager.Instance.IsUnlocked(ADDITIONAL_GOAL_I))
            return;

        UnlockManager.Instance.OnUnlockChanged -= AddGoalsTier1;

        GoalList.Add(new WinrateBandGoal(49f, 54f, 3, GoalDifficulty.Hard, GoalType.Balance));
        GoalList.Add(new SingleStarGoal(55f, GoalDifficulty.Normal, GoalType.Balance));
        GoalList.Add(new MobilityPatchGoal(GoalDifficulty.Easy, GoalType.Patch));
        GoalList.Add(new NoAttackPatchGoal(GoalDifficulty.Easy, GoalType.Patch));
        GoalList.Add(new MinPickRateGoal((100f / RuntimeCharacterManager.Instance.CharacterCount) * 0.65f, GoalDifficulty.Hard, GoalType.Meta));
    }

    private void AddGoalsTier2()
    {
        if (!UnlockManager.Instance.IsUnlocked(ADDITIONAL_GOAL_II))
            return;

        UnlockManager.Instance.OnUnlockChanged -= AddGoalsTier2;
        GoalList.Add(new BottomToTopGoal(AnalysisManager.Instance.GetLowestCharacter(AnalysisItem.Winrate, true), 3, GoalDifficulty.Normal, GoalType.Balance));
        GoalList.Add(new PatchCountGoal(3, GoalDifficulty.Normal, GoalType.Patch));
        GoalList.Add(new PrecisionPatchGoal(GoalDifficulty.Normal, GoalType.Patch));
        GoalList.Add(new MaxPickRateGoal((100f / RuntimeCharacterManager.Instance.CharacterCount) * 1.15f, GoalDifficulty.Hard, GoalType.Meta));
    }

    private void AddGoalsTier3()
    {
        if (!UnlockManager.Instance.IsUnlocked(ADDITIONAL_GOAL_III))
            return;

        UnlockManager.Instance.OnUnlockChanged -= AddGoalsTier3;
        GoalList.Add(new PredictCharacterWinrateRank(RuntimeCharacterManager.Instance.GetRandomCharacter().OriginCharacter, Random.Range(2, RuntimeCharacterManager.Instance.CharacterCount - 1), GoalDifficulty.Impossible, GoalType.Challenge));
        GoalList.Add(new ReverseMetaGoal(GoalDifficulty.Hard, GoalType.Meta));
        GoalList.Add(new SingleStatPatchGoal(GoalDifficulty.Hard, GoalType.Patch));
    }

    public void SetGoals()
    {
        shuffledGoals = GetRandomGoals(currentGoalCount);

        foreach (DeveloperGoal goal in shuffledGoals)
        {
            goal.Initialize();
        }

        if (IsGoalAvailable)
        {
            GoalUI.SetGoals(shuffledGoals);
            BottomGoalUI.SetText(shuffledGoals);
        }
    }

    private List<DeveloperGoal> GetRandomGoals(int count)
    {
        List<DeveloperGoal> result;

        do
        {
            result = new List<DeveloperGoal>(GoalList);

            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            if (count < result.Count)
                result.RemoveRange(count, result.Count - count);

        } while (IsSameGoals(result, shuffledGoals) && GoalList.Count > count);

        return result;
    }

    private bool IsSameGoals(List<DeveloperGoal> a, List<DeveloperGoal> b)
    {
        if (a == null || b == null)
            return false;

        if (a.Count != b.Count)
            return false;

        return a.All(b.Contains);
    }

    //=========================================================
    // Reroll
    //=========================================================

    public void ChangeGoals()
    {
        if (!IsGoalAvailable)
        {
            IsGoalAvailable = true;
            rerollCount = 0;
        }

        if (!isRerollAvailable)
            return;

        int cost = REROLL_REQUIRE_RESOURCE * rerollCount;

        if (!ResourceManager.Instance.SpendDevelopResource(cost))
            return;

        rerollCount++;

        GoalUI.SetRerollCostValue(REROLL_REQUIRE_RESOURCE * rerollCount);

        SetGoals();

        OnGoalChanged?.Invoke();
    }

    //=========================================================
    // Additional Goal
    //=========================================================

    private void CheckSecondGoal()
    {
        currentGoalCount = 2;

        if (!UnlockManager.Instance.IsUnlocked(ADDITIONAL_SLOT_1))
            return;

        UnlockManager.Instance.OnUnlockChanged -= CheckSecondGoal;

        if (shuffledGoals.Count < 2)
            shuffledGoals = GetRandomGoals(currentGoalCount);

        GoalUI.SetGoals(shuffledGoals[1], 1);
    }

    private void CheckThirdGoal()
    {
        currentGoalCount = 3;

        if (!UnlockManager.Instance.IsUnlocked(ADDITIONAL_SLOT_2))
            return;

        UnlockManager.Instance.OnUnlockChanged -= CheckThirdGoal;

        if (shuffledGoals.Count < 3)
            shuffledGoals = GetRandomGoals(currentGoalCount);

        GoalUI.SetGoals(shuffledGoals[2], 2);
    }

    //=========================================================
    // Confirm
    //=========================================================

    public void ConfirmGoals()
    {
        GoalUI.ShowAlert(false);

        BottomDisplayUI.Instance.GoalPreview.SetText(shuffledGoals);
        BottomDisplayUI.Instance.ShowPreview();

        isGoalConfirmed = true;
        isRerollAvailable = false;

        OnGoalConfirmed?.Invoke();

        SeasonManager.Instance.FinishStart();
    }

    //=========================================================
    // Season
    //=========================================================

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

    //=========================================================
    // Reward
    //=========================================================

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

        float multiplier = UnlockManager.Instance.IsUnlocked(GOAL_REWARD) ? 1.2f : 1f;

        reward.DevelopResource *= (int)multiplier;
        reward.TrustPoint *= (int)multiplier;

        return reward;
    }

    //=========================================================
    // Evaluate
    //=========================================================

    public void EvaluateAllGoals()
    {
        foreach (DeveloperGoal goal in shuffledGoals)
            goal.Evaluate();

        RefreshUI();

        // TODO: UI에 진행상황 연결하기
    }

    public void CalculateGoals()
    {
        Debug.Log("CalculateGoals");

        EvaluateAllGoals();

        foreach (DeveloperGoal goal in shuffledGoals)
        {
            if (goal.IsComplete) ResourceManager.Instance.AddReward(goal.Reward);
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
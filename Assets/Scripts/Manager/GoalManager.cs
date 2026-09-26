using System;
using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance;

    public CharacterDatabase characterDatabase;
    public DeveloperGoalUI GoalUI;
    public BottomGoalPreviewUI BottomGoalUI;

    private readonly List<DeveloperGoal> goalList = new();
    private List<DeveloperGoal> shuffledGoals = new();
    private readonly Dictionary<GoalDifficulty, GoalReward> rewardTable = new();

    // 이번 시즌에 보상을 이미 지급한 목표
    private readonly HashSet<DeveloperGoal> rewardedGoals = new();

    public IReadOnlyList<DeveloperGoal> GetGoals => shuffledGoals;
    public DeveloperGoal SelectedGoal => selectedGoal;

    public bool IsGoalAvailable { get; private set; }
    public bool IsGoalSet => isGoalConfirmed;

    [SerializeField] private int currentGoalCount = 1;

    private DeveloperGoal selectedGoal;

    private int rerollCount;

    private bool isRerollAvailable;
    private bool isGoalConfirmed;

    private const int REROLL_REQUIRE_RESOURCE = 10;

    private const int GOAL_REWARD = 2011;
    private const int ADDITIONAL_SLOT_1 = 2021;
    private const int ADDITIONAL_SLOT_2 = 2022;
    private const int FREE_REROLL = 2031;

    private const int ADDITIONAL_GOAL_I = 2041;
    private const int ADDITIONAL_GOAL_II = 2042;
    private const int ADDITIONAL_GOAL_III = 2043;

    public event Action OnGoalChanged;
    public event Action OnGoalConfirmed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        IsGoalAvailable = false;
        currentGoalCount = 1;

        GenerateRewards();
        ResetRerollCount();
    }

    private void Start()
    {
        GenerateGoals();
        SyncUnlocks();

        SetGoals();

        GoalUI.Initialize(shuffledGoals, this);
        BottomGoalUI.Initialize(this);

        UpdateRerollCostUI();

        UnlockManager.Instance.OnUnlockChanged += HandleUnlockChanged;
    }

    private void OnDestroy()
    {
        if (UnlockManager.Instance != null)
            UnlockManager.Instance.OnUnlockChanged -= HandleUnlockChanged;

        if (Instance == this)
            Instance = null;
    }

    //=========================================================
    // Goal Generation
    //=========================================================

    public void GenerateGoals()
    {
        goalList.Clear();

        goalList.Add(new NerfTopGoal(GoalDifficulty.Easy, GoalType.Challenge));

        goalList.Add(new SpecificCharacterWinrateGoal(40, 60, GoalDifficulty.Easy, GoalType.Challenge));
    }

    private void SyncUnlocks()
    {
        if (UnlockManager.Instance == null)
            return;

        if (UnlockManager.Instance.IsUnlocked(ADDITIONAL_SLOT_1))
            currentGoalCount = Mathf.Max(currentGoalCount, 2);

        if (UnlockManager.Instance.IsUnlocked(ADDITIONAL_SLOT_2))
            currentGoalCount = Mathf.Max(currentGoalCount, 3);

        AddUnlockedGoals();
    }

    private void HandleUnlockChanged()
    {
        if (UnlockManager.Instance == null)
            return;

        int previousGoalCount = currentGoalCount;
        int previousGoalListCount = goalList.Count;

        SyncUnlocks();

        bool goalSlotChanged = currentGoalCount != previousGoalCount;
        bool goalPoolChanged = goalList.Count != previousGoalListCount;

        if (goalSlotChanged || goalPoolChanged || !isGoalConfirmed)
        {
            SetGoals();

            if (IsGoalAvailable)
                OnGoalChanged?.Invoke();
        }
    }

    private void AddUnlockedGoals()
    {
        if (UnlockManager.Instance.IsUnlocked(ADDITIONAL_GOAL_I))
        {
            AddGoalOnce(new WinrateBandGoal(49f, 54f, 3, GoalDifficulty.Hard, GoalType.Balance));
            AddGoalOnce(new SingleStarGoal(55f, GoalDifficulty.Normal, GoalType.Balance));
            AddGoalOnce(new MobilityPatchGoal(GoalDifficulty.Easy, GoalType.Patch));
            AddGoalOnce(new NoAttackPatchGoal(GoalDifficulty.Easy, GoalType.Patch));

            int characterCount = RuntimeCharacterManager.Instance.CharacterCount;

            if (characterCount > 0)
            {
                AddGoalOnce(new MinPickRateGoal(
                    (100f / characterCount) * 0.65f,
                    GoalDifficulty.Hard,
                    GoalType.Meta));
            }
        }

        if (UnlockManager.Instance.IsUnlocked(ADDITIONAL_GOAL_II))
        {
            AddGoalOnce(new BottomToTopGoal(
                AnalysisManager.Instance.GetLowestCharacter(AnalysisItem.Winrate, true),
                3,
                GoalDifficulty.Normal,
                GoalType.Balance));

            AddGoalOnce(new PatchCountGoal(
                3,
                GoalDifficulty.Normal,
                GoalType.Patch));

            AddGoalOnce(new PrecisionPatchGoal(
                GoalDifficulty.Normal,
                GoalType.Patch));

            int characterCount = RuntimeCharacterManager.Instance.CharacterCount;

            if (characterCount > 0)
            {
                AddGoalOnce(new MaxPickRateGoal(
                    (100f / characterCount) * 1.15f,
                    GoalDifficulty.Hard,
                    GoalType.Meta));
            }
        }

        if (UnlockManager.Instance.IsUnlocked(ADDITIONAL_GOAL_III))
        {
            int characterCount = RuntimeCharacterManager.Instance.CharacterCount;

            if (characterCount > 2)
            {
                AddGoalOnce(new PredictCharacterWinrateRank(
                    RuntimeCharacterManager.Instance.GetRandomCharacter().OriginCharacter,
                    UnityEngine.Random.Range(2, characterCount - 1),
                    GoalDifficulty.Impossible,
                    GoalType.Challenge));
            }

            AddGoalOnce(new ReverseMetaGoal(
                GoalDifficulty.Hard,
                GoalType.Meta));

            AddGoalOnce(new SingleStatPatchGoal(
                GoalDifficulty.Hard,
                GoalType.Patch));
        }
    }

    private void AddGoalOnce(DeveloperGoal goal)
    {
        if (!goalList.Contains(goal))
            goalList.Add(goal);
    }

    //=========================================================
    // Goal Selection
    //=========================================================

    public void SetGoals()
    {
        shuffledGoals = GetRandomGoals(currentGoalCount);

        selectedGoal = null;

        foreach (DeveloperGoal goal in shuffledGoals)
            goal.Refresh();

        if (IsGoalAvailable)
        {
            GoalUI.SetGoals(shuffledGoals);
        }
    }

    private List<DeveloperGoal> GetRandomGoals(int count)
    {
        List<DeveloperGoal> result = new(goalList);

        // Fisher-Yates shuffle
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        int selectedCount = Mathf.Clamp(count, 0, result.Count);

        if (result.Count > selectedCount)
            result.RemoveRange(
                selectedCount,
                result.Count - selectedCount);

        return result;
    }

    public void SelectGoal(int index)
    {
        if (isGoalConfirmed)
            return;

        if (index < 0 || index >= shuffledGoals.Count)
            return;

        selectedGoal = shuffledGoals[index];

        GoalUI.SetSelectedGoal(index);

        Debug.Log($"선택한 목표: {selectedGoal.Title}");
    }

    //=========================================================
    // Reroll
    //=========================================================

    public void ChangeGoals()
    {
        if (isGoalConfirmed || !isRerollAvailable)
            return;

        if (!IsGoalAvailable)
        {
            IsGoalAvailable = true;
            rerollCount = 0;
        }

        int cost = REROLL_REQUIRE_RESOURCE * rerollCount;

        if (!ResourceManager.Instance.SpendDevelopResource(cost))
            return;

        rerollCount++;

        SetGoals();
        UpdateRerollCostUI();

        OnGoalChanged?.Invoke();
    }

    private void UpdateRerollCostUI()
    {
        if (GoalUI != null)
            GoalUI.SetRerollCostValue(
                REROLL_REQUIRE_RESOURCE * rerollCount);
    }

    //=========================================================
    // Confirm
    //=========================================================

    public void ConfirmGoals()
    {
        if (isGoalConfirmed || selectedGoal == null)
            return;

        if (GoalUI != null)
            GoalUI.ShowAlert(false);

        if (BottomDisplayUI.Instance != null)
        {
            BottomDisplayUI.Instance.GoalPreview.SetText(selectedGoal);
            BottomDisplayUI.Instance.ShowPreview();
        }

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
        IsGoalAvailable = false;
        isGoalConfirmed = false;

        selectedGoal = null;

        rewardedGoals.Clear();

        if (GoalUI != null)
            GoalUI.ShowAlert(true);

        ResetRerollCount();

        GenerateGoals();
        SyncUnlocks();
        SetGoals();
    }

    public void ResetRerollCount()
    {
        isRerollAvailable = true;
        isGoalConfirmed = false;

        selectedGoal = null;

        if (UnlockManager.Instance == null)
        {
            rerollCount = 1;
        }
        else
        {
            rerollCount =
                UnlockManager.Instance.IsUnlocked(FREE_REROLL)
                ? 0
                : 1;
        }

        UpdateRerollCostUI();
    }

    //=========================================================
    // Reward
    //=========================================================

    private void GenerateRewards()
    {
        rewardTable.Clear();

        rewardTable.Add(
            GoalDifficulty.Easy,
            new GoalReward(100, 25));

        rewardTable.Add(
            GoalDifficulty.Normal,
            new GoalReward(150, 35));

        rewardTable.Add(
            GoalDifficulty.Hard,
            new GoalReward(300, 50));

        rewardTable.Add(
            GoalDifficulty.Impossible,
            new GoalReward(500, 75));
    }

    public GoalReward GetReward(GoalDifficulty difficulty)
    {
        if (!rewardTable.TryGetValue(
                difficulty,
                out GoalReward reward))
        {
            return new GoalReward(0, 0);
        }

        float multiplier =
            UnlockManager.Instance != null &&
            UnlockManager.Instance.IsUnlocked(GOAL_REWARD)
                ? 1.2f
                : 1f;

        return new GoalReward(
            Mathf.RoundToInt(
                reward.DevelopResource * multiplier),

            Mathf.RoundToInt(
                reward.TrustPoint * multiplier));
    }

    //=========================================================
    // Evaluate
    //=========================================================

    public void EvaluateAllGoals()
    {
        if (selectedGoal == null)
            return;

        selectedGoal.Evaluate();

        RefreshUI();
    }

    public void CalculateGoals()
    {
        if (!isGoalConfirmed || selectedGoal == null)
            return;

        selectedGoal.Evaluate();

        if (selectedGoal.IsComplete && rewardedGoals.Add(selectedGoal))
        {
            ResourceManager.Instance.AddReward(selectedGoal.Reward);

            Debug.Log($"목표 완료: {selectedGoal.Title}");
        }
        else
        {
            Debug.Log($"목표 실패: {selectedGoal.Title}");
            EncounterManager.Instance.ApplyEncounter(EncounterManager.Instance.GetRandomEncounterNegative());
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (GoalUI != null)
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
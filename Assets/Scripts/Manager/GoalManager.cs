using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using NUnit.Framework;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance;
    public CharacterDatabase characterDatabase;

    public DeveloperGoalUI GoalUI;

    private List<DeveloperGoal> GoalList = new();
    private List<DeveloperGoal> currentGoals = new();
    private Dictionary<GoalDifficulty, GoalReward> RewardTable = new();

    private int rerollCount;
    private const int REROLL_REQUIRE_RESOURCE = 10;

    private bool isRerollAvailable;

    void Awake()
    {
        Instance = this;
        GenerateRewards();
        GenerateGoals();
        ResetRerollCount();
    }

    void Start()
    {
        SetGoals();
        GoalUI.Initialize(currentGoals, this);
    }

    public void GenerateGoals()
    {
        GoalList.Clear();

        GoalList.Add(new BottomToTopGoal(AnalysisManager.Instance.GetLowestCharacter(AnalysisItem.Winrate), 3, GoalDifficulty.Normal, GoalType.Balance));
        GoalList.Add(new VeteranMakerGoal(57f, GoalDifficulty.Normal, GoalType.Balance));
        GoalList.Add(new WinrateBandGoal(49f, 54f, 4, GoalDifficulty.Hard, GoalType.Balance));
        GoalList.Add(new WinrateRangeGoal(45f, 55f, GoalDifficulty.Impossible, GoalType.Balance));

        GoalList.Add(new PredictCharacterWinrateRank(characterDatabase.GetRandomCharacter(), Random.Range(1, 9), GoalDifficulty.Impossible, GoalType.Challenge));
        GoalList.Add(new SpecificCharacterWinrateGoal(40, 60, characterDatabase.GetRandomCharacter(), GoalDifficulty.Hard, GoalType.Challenge));

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

    public void SetGoals()
    {
        currentGoals.Clear();

        List<DeveloperGoal> shuffled = new(GoalList);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        for (int i = 0; i < Mathf.Min(3, shuffled.Count); i++)
        {
            currentGoals.Add(shuffled[i]);
        }

        GoalUI.SetGoals(currentGoals);
    }

    public void ChangeGoals()
    {
        if (!isRerollAvailable) return;
        if (!ResourceManager.Instance.SpendDevelopResource(REROLL_REQUIRE_RESOURCE * rerollCount)) return;

        GoalUI.SetRerollCostValue(REROLL_REQUIRE_RESOURCE * (rerollCount + 1));
        rerollCount++;
        SetGoals();
    }

    public void ConfirmGoals()
    {
        isRerollAvailable = false;
        SeasonManager.Instance.FinishStart();
    }

    public void ResetRerollCount()
    {
        isRerollAvailable = true;
        rerollCount = 0;
    }

    private void GenerateRewards()
    {
        RewardTable.Add(GoalDifficulty.Easy, new GoalReward(30, 5));
        RewardTable.Add(GoalDifficulty.Normal, new GoalReward(40, 7));
        RewardTable.Add(GoalDifficulty.Hard, new GoalReward(60, 10));
        RewardTable.Add(GoalDifficulty.Impossible, new GoalReward(80, 15));
    }

    public GoalReward GetReward(GoalDifficulty difficulty)
    {
        RewardTable.TryGetValue(difficulty, out GoalReward reward);
        return reward;
    }

    public void EvaluateAllGoals()
    {
        foreach (DeveloperGoal goal in currentGoals)
        {
            goal.Evaluate();
        }

        RefreshUI();
        //todo ui에 진행상황 연결하기
    }

    public void CalculateGoals()
    {
        EvaluateAllGoals();

        foreach (DeveloperGoal goal in currentGoals)
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
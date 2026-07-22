using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance;
    public CharacterDatabase characterDatabase;

    public DeveloperGoalUI GoalUI;

    private List<DeveloperGoal> GoalList = new();
    private List<DeveloperGoal> currentGoals = new();
    private Dictionary<GoalDifficulty, GoalReward> RewardTable = new();

    void Awake()
    {
        Instance = this;
        GenerateRewards();
        GenerateGoals();
        SetGoals();

        GoalUI.Initialize(currentGoals);
    }

    public void GenerateGoals()
    {
        GoalList.Clear();

        GoalList.Add(new WinrateRangeGoal(5f, 95f, GoalDifficulty.Easy));
        GoalList.Add(new MaxPickRateGoal(20f, GoalDifficulty.Normal));
        GoalList.Add(new SpecificCharacterWinrateGoal(40, 60, characterDatabase.GetRandomCharacter(), GoalDifficulty.Hard));
        GoalList.Add(new PredictCharacterWinrateRank(characterDatabase.GetRandomCharacter(), Random.Range(0, 10), GoalDifficulty.Impossible));
    }

    private void SetGoals()
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

        GoalUI.Initialize(currentGoals);
    }

    private void GenerateRewards()
    {
        RewardTable.Add(GoalDifficulty.Easy, new GoalReward(100, 3));
        RewardTable.Add(GoalDifficulty.Normal, new GoalReward(150, 5));
        RewardTable.Add(GoalDifficulty.Hard, new GoalReward(220, 10));
        RewardTable.Add(GoalDifficulty.Impossible, new GoalReward(300, 15));
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
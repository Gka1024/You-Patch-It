using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance;
    public CharacterDatabase characterDatabase;

    public DeveloperGoalUI GoalUI;

    private List<DeveloperGoal> currentGoals = new();

    void Awake()
    {
        Instance = this;
        GenerateGoals();
        GoalUI.Initialize(currentGoals);
    }

    public void GenerateGoals()
    {
        currentGoals.Clear();

        currentGoals.Add(new WinrateRangeGoal(45f, 55f));
        currentGoals.Add(new MaxPickRateGoal(20f));
        currentGoals.Add(new SpecificCharacterWinrateGoal(40, 60, characterDatabase.GetRandomCharacter()));
    }

    public int GetCompletedReward()
    {
        return 0;
    }
}
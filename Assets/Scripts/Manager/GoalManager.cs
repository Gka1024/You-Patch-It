using System.Collections.Generic;
using UnityEngine;

public class GoalManager: MonoBehaviour
{
    public static GoalManager Instance;

    public GameObject GoalUI;

    private List<DeveloperGoal> currentGoals;

    void Awake()
    {
        Instance = this;
    }
}
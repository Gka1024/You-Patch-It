using UnityEditor.U2D.Animation;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;

    [SerializeField] private RuntimeCharacterManager runtimeCharacterManager;
    [SerializeField] private StatisticsManager statisticsManager;

    private void Awake()
    {
        characterDatabase.Initialize();
        runtimeCharacterManager.Initialize(characterDatabase);
        statisticsManager.Initialize(characterDatabase);
    }
}
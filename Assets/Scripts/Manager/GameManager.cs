using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;

    [SerializeField] private RuntimeCharacterManager runtimeCharacterManager;
    [SerializeField] private StatisticsManager statisticsManager;
    [SerializeField] private ResultManager resultManager;

    private void Awake()
    {
        characterDatabase.Initialize();
        runtimeCharacterManager.Initialize(characterDatabase);
        statisticsManager.Initialize(characterDatabase);
        resultManager.Initialize(characterDatabase);
    }
}
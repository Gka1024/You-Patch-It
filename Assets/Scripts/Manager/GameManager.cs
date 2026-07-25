using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private CharacterDatabase characterDatabase;

    [SerializeField] private RuntimeCharacterManager runtimeCharacterManager;
    [SerializeField] private StatisticsManager statisticsManager;
    [SerializeField] private ResultManager resultManager;

    private void Awake()
    {
        Instance = this;

        characterDatabase.Initialize();
        runtimeCharacterManager.Initialize(characterDatabase);
        statisticsManager.Initialize(characterDatabase);
        resultManager.Initialize(characterDatabase);
    }

    public void GameOver()
    {
        UIManager.Instance.upDisplayUI.GameOverUI.SetActive(true);
    }
}
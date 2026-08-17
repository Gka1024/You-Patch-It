#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.UI;

public class GameExitPopupUI : MonoBehaviour
{
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button denyButton;

    void Awake()
    {
        confirmButton.onClick.AddListener(ExitGame);
        denyButton.onClick.AddListener(KeepPlay);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    private void KeepPlay()
    {
        gameObject.SetActive(false);
    }
}
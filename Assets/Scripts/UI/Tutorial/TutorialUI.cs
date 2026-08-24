using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{   
    [SerializeField] private Button SkipTutorialButton;

    void Awake()
    {
        SkipTutorialButton.onClick.AddListener(SkipTutorial);
    }

    private void SkipTutorial()
    {
        Destroy(this.gameObject);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ResolutionPopupUI : MonoBehaviour
{
    [SerializeField] private Button confirmButton;

    void Awake()
    {
        confirmButton.onClick.AddListener(MakeUIInvisible);
    }

    private void MakeUIInvisible()
    {
        gameObject.SetActive(false);
    }
}
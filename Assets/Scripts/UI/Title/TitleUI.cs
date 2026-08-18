using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    public Button StartButton;

    void Awake()
    {
        StartButton.onClick.AddListener(DestroyThis);
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}

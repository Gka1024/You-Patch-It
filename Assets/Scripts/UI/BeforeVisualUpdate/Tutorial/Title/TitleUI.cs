using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    public Button StartButton;
    public Button InstructionButton;
    public Button ResolutionButton;

    public GameObject InstructionObject;
    public GameObject ResolutionSetting;

    void Awake()
    {
        StartButton.onClick.AddListener(DestroyThis);
        InstructionButton.onClick.AddListener(ShowInstruction);
        ResolutionButton.onClick.AddListener(ShowResolutionSetting);
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }

    private void ShowInstruction()
    {
        InstructionObject.SetActive(true);
    }

    private void ShowResolutionSetting()
    {
        ResolutionSetting.SetActive(true);
    }
}

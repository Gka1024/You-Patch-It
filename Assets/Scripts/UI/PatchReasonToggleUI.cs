using UnityEngine;
using UnityEngine.UI;

public class PatchReasonToggleUI : MonoBehaviour
{
    [SerializeField] private Toggle toggle;

    public PatchReason PatchReason;
    public bool IsOn => toggle.isOn;

    public Toggle Toggle => toggle;

    public void ResetToggle()
    {
        toggle.SetIsOnWithoutNotify(false);
    }

}
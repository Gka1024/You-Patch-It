using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatchReasonPopupUI : MonoBehaviour
{
    [SerializeField] private List<PatchReasonToggleUI> toggles;
    [SerializeField] private Button confirmButton;

    private void Awake()
    {
        foreach (var toggle in toggles)
        {
            toggle.Toggle.onValueChanged.AddListener(_ => RefreshButton());
        }

        RefreshButton();
    }

    public void Show()
    {
        foreach (var toggle in toggles)
            toggle.ResetToggle();

        RefreshButton();

        gameObject.SetActive(true);
    }

    public List<PatchReason> GetPatchReasons()
    {
        List<PatchReason> reasons = new();

        foreach (var toggle in toggles)
        {
            if (toggle.IsOn)
                reasons.Add(toggle.PatchReason);
        }

        return reasons;
    }

    private void RefreshButton()
    {
        confirmButton.interactable = toggles.Exists(x => x.IsOn);
    }
}
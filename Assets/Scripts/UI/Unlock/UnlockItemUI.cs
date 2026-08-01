using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockItemUI : MonoBehaviour
{
    [SerializeField] private UnlockUI unlockUI;

    [SerializeField] private int unlockID;

    [SerializeField] private Button selfClickButton;

    [SerializeField] private Image icon;
    [SerializeField] private Image UnlockImage;

    [SerializeField] private UnlockCategory category;

    public UnlockData UnlockData { get; private set; }

    private void Awake()
    {
        selfClickButton.onClick.AddListener(SelfClick);
    }

    public void Register(UnlockUI ui)
    {
        unlockUI = ui;
        UnlockData = UnlockManager.Instance.GetUnlockData(unlockID);
        Refresh();
    }

    public void Refresh()
    {
        if(UnlockData == null) return;

        if(icon != null)
        {
            icon.sprite = UnlockData.icon;
        }

        if(UnlockImage != null)
        {
            UnlockImage.enabled = !UnlockManager.Instance.IsUnlocked(UnlockData);
        }
    }

    private void SelfClick()
    {
        unlockUI.SetInspector(this);
    }

    public void Unlock()
    {
        UnlockManager.Instance.Unlock(UnlockData);
    }

}
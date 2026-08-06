using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockItemUI : MonoBehaviour
{
    [SerializeField] private UnlockUI unlockUI;

    [SerializeField] private int unlockID;

    [SerializeField] private Button selfClickButton;

    [SerializeField] private Image BackgroundImage;
    [SerializeField] private Image Icon;
    [SerializeField] private Sprite UnlockSprite;

    [SerializeField] private UnlockCategory category;

    public UnlockData UnlockData { get; private set; }

    private void Awake()
    {
        selfClickButton.onClick.AddListener(SelfClick);
        BackgroundImage = gameObject.GetComponentInChildren<Image>();
    }

    public void Register(UnlockUI ui)
    {
        unlockUI = ui;
        UnlockData = UnlockManager.Instance.GetUnlockData(unlockID);
        UnlockSprite = ui.UnlockSprite;
        Refresh();
    }

    public void Refresh()
    {
        if (UnlockData == null) return;
    }

    private void SelfClick()
    {
        unlockUI.SetInspector(this);
    }

    private void SetUnlockedImage()
    {
        BackgroundImage.sprite = UnlockSprite;
    }

    public void Unlock()
    {
        if (UnlockManager.Instance.Unlock(UnlockData))
        {
            if (UnlockData.NextData != null)
            {
                this.UnlockData = UnlockData.NextData;
            }
            else
            {
                SetUnlockedImage();
            }

            SelfClick();
            Refresh();
        }
    }

}
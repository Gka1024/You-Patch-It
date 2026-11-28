using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DesktopIconUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Button target;
    [SerializeField] private Image selectBackground;
    [SerializeField] private TMP_Text label;

    private static DesktopIconUI selected;

    private void OnEnable()
    {
        SetSelected(false);
    }

    private void OnDisable()
    {
        if (selected == this)
            selected = null;
    }

    public static void ClearSelection()
    {
        if (selected != null)
            selected.SetSelected(false);

        selected = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (selected != this)
        {
            ClearSelection();
            selected = this;
            SetSelected(true);
        }

        if (eventData.clickCount >= 2)
            Open();
    }

    public void Open()
    {
        if (target == null || !target.IsInteractable())
            return;

        DesktopWireFrame.NextOrigin = (RectTransform)transform;
        target.onClick.Invoke();
    }

    private void SetSelected(bool value)
    {
        if (selectBackground != null)
            selectBackground.enabled = value;
    }
}

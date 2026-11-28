using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 공통 창 프레임(DesktopWindow 프리팹)의 동작: 타이틀/아이콘 설정, 닫기, 포커스(활성/비활성 타이틀바 색).
/// 앱 콘텐츠는 프리팹의 Content 아래에 넣는다.
/// </summary>
public class DesktopWindow : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image titleBar;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Button closeButton;
    [SerializeField] private UnityEvent onClosed;

    private static DesktopWindow focused;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        Focus();
    }

    private void OnDisable()
    {
        if (focused == this)
            focused = null;
    }

    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    public void SetIcon(Sprite icon)
    {
        if (iconImage == null)
            return;

        iconImage.sprite = icon;
        iconImage.enabled = icon != null;
    }

    public void Close()
    {
        onClosed.Invoke();
        gameObject.SetActive(false);
    }

    public void Focus()
    {
        if (focused != null && focused != this)
            focused.SetFocused(false);

        focused = this;
        transform.SetAsLastSibling();
        SetFocused(true);
    }

    public void SetFocused(bool value)
    {
        if (titleBar == null)
            return;

        Sprite sprite = value ? activeSprite : inactiveSprite;

        if (sprite != null)
            titleBar.sprite = sprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Focus();
    }
}

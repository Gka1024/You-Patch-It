using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 타이틀바에 붙여 창을 드래그로 옮긴다. 위치는 항상 정수 픽셀로 스냅된다.
/// </summary>
public class DesktopWindowDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform window;
    [SerializeField] private DesktopWindow DesktopWindow;

    private Canvas rootCanvas;
    private Vector2 rawPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (DesktopWindow != null)
            DesktopWindow.Focus();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        rootCanvas = canvas != null ? canvas.rootCanvas : null;
        rawPosition = window.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (window == null || rootCanvas == null)
            return;

        // 화면 픽셀 → 캔버스 유닛. 소수점 이동량은 rawPosition 에 누적하고 표시 위치만 반올림한다.
        rawPosition += eventData.delta / rootCanvas.scaleFactor;
        window.anchoredPosition = new Vector2(Mathf.Round(rawPosition.x), Mathf.Round(rawPosition.y));
    }
}


using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Win95/98 의 창 열기/닫기 연출: 점선 사각형이 시작 위치(아이콘/작업표시줄 버튼)에서
/// 창 크기까지 몇 단계로 '끊겨서' 커진다. 모든 값은 정수 픽셀에 스냅된다.
/// </summary>
public class DesktopWireFrame : MonoBehaviour
{
    public static DesktopWireFrame Instance { get; private set; }

    /// <summary>다음 창이 열릴 때 와이어프레임이 출발할 위치. 아이콘/작업표시줄 버튼이 클릭 시 설정한다.</summary>
    public static RectTransform NextOrigin;

    [SerializeField] private RectTransform wire;

    private Tween tween;
    private Action pendingDone;

    private void Awake()
    {
        Instance = this;

        wire.anchorMin = wire.anchorMax = wire.pivot = new Vector2(0.5f, 0.5f);
        wire.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        tween?.Kill();

        if (Instance == this)
            Instance = null;
    }

    /// <param name="from">시작 사각형 (월드 좌표)</param>
    /// <param name="to">도착 사각형 (월드 좌표)</param>
    public Tween Play(Rect from, Rect to, int steps, float duration, Action onDone)
    {
        Stop(true);

        steps = Mathf.Max(1, steps);
        pendingDone = onDone;

        wire.gameObject.SetActive(true);
        wire.SetAsLastSibling();
        Apply(from, to, 1f / steps);

        tween = DOVirtual.Float(0f, 1f, duration, v =>
            {
                int index = Mathf.Min(steps - 1, Mathf.FloorToInt(v * steps));
                Apply(from, to, (index + 1f) / steps);
            })
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnComplete(() => Stop(true));

        return tween;
    }

    /// <summary>진행 중인 연출을 멈추고 와이어를 숨긴다. invokePending 이면 완료 콜백(창 표시)을 즉시 실행한다.</summary>
    public void Stop(bool invokePending)
    {
        tween?.Kill();
        tween = null;

        if (wire != null)
            wire.gameObject.SetActive(false);

        Action done = pendingDone;
        pendingDone = null;

        if (invokePending)
            done?.Invoke();
    }

    private void Apply(Rect from, Rect to, float t)
    {
        RectTransform parent = (RectTransform)wire.parent;

        Vector2 centerWorld = Vector2.Lerp(from.center, to.center, t);
        Vector2 sizeWorld = Vector2.Lerp(from.size, to.size, t);

        Vector3 scale = parent.lossyScale;
        Vector2 local = parent.InverseTransformPoint(new Vector3(centerWorld.x, centerWorld.y, parent.position.z));
        Vector2 size = new Vector2(sizeWorld.x / scale.x, sizeWorld.y / scale.y);

        // 정수 픽셀 스냅 (짝수 크기로 맞춰 중심 반픽셀 오차 방지)
        size.x = Mathf.Max(4f, Mathf.Round(size.x / 2f) * 2f);
        size.y = Mathf.Max(4f, Mathf.Round(size.y / 2f) * 2f);
        Vector2 pos = local - parent.rect.center;

        wire.sizeDelta = size;
        wire.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }
}

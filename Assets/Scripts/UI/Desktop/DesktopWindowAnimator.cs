using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 기존 코드가 SetActive(true) 로 패널을 켜는 순간(OnEnable) 자동으로 재생되는 열기 연출.
/// DashBoardUI 등 기존 스크립트를 수정하지 않고 패널/팝업 오브젝트에 붙이기만 하면 된다.
///
///  Wireframe  : 점선 사각형이 아이콘 위치에서 창 크기까지 단계적으로 커진 뒤 창이 나타난다. (Win95/98)
///  RevealOnly : Wireframe 과 같은 시간 동안만 숨겼다가 나타난다. (같이 켜지는 보조 패널용)
///  Popup      : 즉시 표시 + 짧게 흔들리며 타이틀바가 깜빡인다. (오류/확인 팝업)
/// </summary>
[DisallowMultipleComponent]
public class DesktopWindowAnimator : MonoBehaviour
{
    public enum Mode { Wireframe, RevealOnly, Popup }

    [SerializeField] private Mode mode = Mode.Wireframe;

    [Tooltip("Wireframe 이 향할 영역. 비우면 이 오브젝트의 RectTransform.")]
    [SerializeField] private RectTransform wireframeTarget;

    [SerializeField] private int steps = 4;
    [SerializeField] private float duration = 0.16f;

    [Header("Popup (선택)")]
    [SerializeField] private Image titleBar;
    [SerializeField] private Sprite titleActive;
    [SerializeField] private Sprite titleInactive;
    [SerializeField] private float shakeStepTime = 0.04f;

    private static readonly int[] ShakeOffsets = { -2, 2, -1, 1, 0 };

    private RectTransform rect;
    private CanvasGroup group;
    private Tween tween;
    private bool wireframePlaying;
    private bool shaking;
    private Vector2 basePosition;

    private void Awake()
    {
        rect = (RectTransform)transform;
        group = GetComponent<CanvasGroup>();

        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        Cleanup();

        switch (mode)
        {
            case Mode.Wireframe: PlayWireframe(); break;
            case Mode.RevealOnly: PlayReveal(); break;
            case Mode.Popup: PlayPopup(); break;
        }
    }

    private void OnDisable()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        tween?.Kill();
        tween = null;

        if (wireframePlaying && DesktopWireFrame.Instance != null)
            DesktopWireFrame.Instance.Stop(false);

        wireframePlaying = false;

        if (shaking)
        {
            rect.anchoredPosition = basePosition;
            shaking = false;
        }

        if (group != null)
            SetHidden(false);
    }

    private void SetHidden(bool hidden)
    {
        group.alpha = hidden ? 0f : 1f;
        group.blocksRaycasts = !hidden;
        group.interactable = !hidden;
    }

    // ------------------------------------------------------------ Wireframe

    private void PlayWireframe()
    {
        DesktopWireFrame fx = DesktopWireFrame.Instance;

        if (fx == null)
        {
            PlayReveal();
            return;
        }

        SetHidden(true);

        // 레이아웃이 계산된 다음 프레임에 목표 사각형을 읽는다.
        tween = DOVirtual.DelayedCall(0f, () =>
        {
            Canvas.ForceUpdateCanvases();

            RectTransform target = wireframeTarget != null ? wireframeTarget : rect;
            Rect to = WorldRect(target);

            RectTransform originRect = DesktopWireFrame.NextOrigin;
            DesktopWireFrame.NextOrigin = null;

            Rect from = originRect != null
                ? WorldRect(originRect)
                : new Rect(to.center.x - 8f, to.center.y - 6f, 16f, 12f);

            wireframePlaying = true;
            tween = fx.Play(from, to, steps, duration, () =>
            {
                wireframePlaying = false;
                SetHidden(false);
            });
        }, true);
    }

    // ------------------------------------------------------------ Reveal

    private void PlayReveal()
    {
        SetHidden(true);
        tween = DOVirtual.DelayedCall(duration, () => SetHidden(false), true);
    }

    // ------------------------------------------------------------ Popup

    private void PlayPopup()
    {
        SetHidden(false);

        basePosition = rect.anchoredPosition;
        shaking = true;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        bool canFlash = titleBar != null && titleActive != null && titleInactive != null;

        for (int i = 0; i < ShakeOffsets.Length; i++)
        {
            int index = i;
            seq.AppendCallback(() =>
            {
                rect.anchoredPosition = basePosition + new Vector2(ShakeOffsets[index], 0f);

                if (canFlash)
                    titleBar.sprite = index % 2 == 0 ? titleInactive : titleActive;
            });
            seq.AppendInterval(shakeStepTime);
        }

        seq.OnComplete(() =>
        {
            rect.anchoredPosition = basePosition;
            shaking = false;

            if (canFlash)
                titleBar.sprite = titleActive;
        });

        tween = seq;
    }

    private static Rect WorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
    }
}

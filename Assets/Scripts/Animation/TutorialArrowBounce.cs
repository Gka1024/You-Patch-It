using DG.Tweening;
using UnityEngine;

public class TutorialArrowBounce : MonoBehaviour
{
    public enum ArrowDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [Header("Settings")]
    [SerializeField] private RectTransform arrow;
    [SerializeField] private ArrowDirection direction = ArrowDirection.Down;

    [Header("Animation")]
    [SerializeField] private float moveDistance = 12f;
    [SerializeField] private float moveDuration = 0.4f;

    private Tween bounceTween;
    private Vector2 initialPosition;

    private void OnEnable()
    {
        initialPosition = arrow.anchoredPosition;
        PlayAnimation();
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    private void PlayAnimation()
    {
        StopAnimation();

        Vector2 offset = direction switch
        {
            ArrowDirection.Up => Vector2.up,
            ArrowDirection.Down => Vector2.down,
            ArrowDirection.Left => Vector2.left,
            ArrowDirection.Right => Vector2.right,
            _ => Vector2.down
        };

        Vector2 targetPosition = initialPosition + offset * moveDistance;

        bounceTween = arrow.DOAnchorPos(targetPosition, moveDuration)
            .SetEase(Ease.OutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopAnimation()
    {
        bounceTween?.Kill();
        bounceTween = null;

        if (arrow != null)
            arrow.anchoredPosition = initialPosition;
    }

    public void SetDirection(ArrowDirection newDirection)
    {
        direction = newDirection;

        if (gameObject.activeInHierarchy)
            PlayAnimation();
    }
}
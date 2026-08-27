using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstantDescriptionUI : MonoBehaviour
{
    public TMP_Text NameText;
    public TMP_Text DescriptionText;

    [Header("Size")]
    [SerializeField] private float baseHeight = 100f;
    [SerializeField] private float lineHeight = 25f;

    [Header("Position")]
    [SerializeField] private Vector2 offset = new Vector2(20f, -20f);
    [SerializeField] private float screenMargin = 10f;

    [SerializeField] private RectTransform DescriptionRect;
    [SerializeField] private RectTransform BackgroundRect;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasRectTransform =
            GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    public void Initialize(string name, string desc)
    {
        NameText.text = name;
        DescriptionText.text = desc;

        UpdateHeight();
    }

    private void UpdateHeight()
    {
        DescriptionText.ForceMeshUpdate();

        float textHeight = DescriptionText.preferredHeight;

        float height = Mathf.Max(
            baseHeight,
            baseHeight + textHeight - lineHeight
        );

        DescriptionRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            height
        );

        // 배경 이미지도 최종 크기로 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate(BackgroundRect);
    }

    public void SetPosition(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            screenPosition,
            null,
            out Vector2 localPosition
        );

        Vector2 finalPosition = localPosition + offset;

        Vector2 popupSize = BackgroundRect.rect.size;
        Vector2 canvasSize = canvasRectTransform.rect.size;

        float halfWidth = popupSize.x * 0.5f;
        float halfHeight = popupSize.y * 0.5f;

        float canvasLeft = -canvasSize.x * 0.5f;
        float canvasRight = canvasSize.x * 0.5f;
        float canvasBottom = -canvasSize.y * 0.5f;
        float canvasTop = canvasSize.y * 0.5f;

        // ====================================================
        // 좌우
        // ====================================================

        // 오른쪽으로 넘어가면 X 방향 반전
        if (finalPosition.x + halfWidth > canvasRight)
        {
            finalPosition.x = localPosition.x - Mathf.Abs(offset.x);
        }
        // 왼쪽으로 넘어가면 X 방향 반전
        else if (finalPosition.x - halfWidth < canvasLeft)
        {
            finalPosition.x = localPosition.x + Mathf.Abs(offset.x);
        }

        // ====================================================
        // 상하
        // ====================================================

        float popupTop = finalPosition.y + halfHeight;
        float popupBottom = finalPosition.y - halfHeight;

        // 위쪽으로 넘어간 만큼 아래로 이동
        if (popupTop + screenMargin / 2 > canvasTop)
        {
            float overflow = popupTop - canvasTop;

            finalPosition.y -= overflow + screenMargin;
        }
        // 아래쪽으로 넘어간 만큼 위로 이동
        else if (popupBottom + screenMargin / 2 < canvasBottom)
        {
            float overflow = canvasBottom - popupBottom;

            finalPosition.y += overflow + screenMargin;
        }

        rectTransform.localPosition = finalPosition;
    }
}
using TMPro;
using UnityEngine;

public class InstantDescriptionUI : MonoBehaviour
{
    public TMP_Text NameText;
    public TMP_Text DescriptionText;

    [SerializeField] private Vector2 offset = new Vector2(20f, -20f);

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    public void Initialize(string name, string desc)
    {
        NameText.text = name;
        DescriptionText.text = desc;
    }

    public void SetPosition(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            screenPosition,
            null,
            out Vector2 localPosition
        );

        rectTransform.localPosition = localPosition + offset;
    }
}
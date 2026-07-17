using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterTableHeaderUI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private CharacterTableUI table;
    [SerializeField] private AnalysisItem sortItem;
    public AnalysisItem Item => sortItem;

    [SerializeField] private Button textButton;
    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_Text sortArrow;

    void Awake()
    {
        textButton.onClick.AddListener(OnClick);
    }

    public void Initialize(CharacterTableUI ui)
    {
        this.table = ui;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        table.OnClickHeader(sortItem);
    }

    public void OnClick()
    {

    }

    public void Refresh(bool selected, SortDirection direction)
    { // ▲▼

        if (!selected)
        {
            sortArrow.text = "";
            return;
        }

        sortArrow.text = direction == SortDirection.Descending ? "▼" : "▲";
    }


}

public enum SortDirection
{
    Ascending,
    Descending
}
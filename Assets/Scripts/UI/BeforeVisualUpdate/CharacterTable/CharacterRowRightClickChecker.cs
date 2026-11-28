using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterRowClickChecker : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CharacterRowUI row;

    public void Initialize(CharacterRowUI row)
    {
        this.row = row;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            row.OnClickLeft();
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            row.OnClickRight();
        }
    }
}
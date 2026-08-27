using UnityEngine;
using UnityEngine.EventSystems;

public class DescriptionPopupUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string Name;
    [TextArea] public string Description;

    public void SetText(string name, string desc)
    {
        Name = name;
        Description = desc;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.SpawnInstantDesc(Name, Description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.DespawnInstantDesc();
    }
}
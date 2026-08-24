using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialIndexUI : MonoBehaviour, IPointerClickHandler
{
    public int index;
    public GameObject[] contents;
    public GameObject nextUI;

    protected virtual void Awake()
    {
        index = 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CanEnterNextPage())
            EnterNextPage();
    }

    protected virtual bool CanEnterNextPage()
    {
        return true;
    }

    protected void EnterNextPage()
    {
        contents[index++].SetActive(false);

        if (index >= contents.Length)
        {
            ShowNextTutorial();
            return;
        }

        contents[index].SetActive(true);
    }

    private void ShowNextTutorial()
    {
        nextUI.SetActive(true);
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleInstructionUI : MonoBehaviour
{
    [SerializeField] private GameObject Panels;
    [SerializeField] private Button PreviousPage;
    [SerializeField] private Button NextPage;
    [SerializeField] private Button RemoveObjectButton;

    [SerializeField] private TMP_Text NumberText;
    [SerializeField] private int numIndex = 1;
    [SerializeField] private int numMax;

    void OnEnable()
    {
        ClearAll();
        ShowPage(1);
    }

    void Awake()
    {
        NextPage.onClick.AddListener(MoveToNext);
        PreviousPage.onClick.AddListener(MoveToPrevious);
        RemoveObjectButton.onClick.AddListener(RemoveObject);
        numMax = Panels.transform.childCount - 1;
    }

    private void ClearAll()
    {
        foreach (Transform child in Panels.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void ShowPage(int page)
    {
        Panels.transform.GetChild(page).transform.gameObject.SetActive(true);
        NumberText.text = page.ToString();
    }

    public void MoveToNext()
    {
        Debug.Log("!@#");
        if (numIndex + 1 <= numMax)
        {
            ClearAll();
            numIndex++;
            ShowPage(numIndex);
        }
    }

    public void MoveToPrevious()
    {
        Debug.Log("!@#");
        if (1 < numIndex)
        {
            ClearAll();
            numIndex--;
            ShowPage(numIndex);
        }
    }

    private void RemoveObject()
    {
        gameObject.SetActive(false);
    }
}

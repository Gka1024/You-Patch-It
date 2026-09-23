using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockUI : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private UnlockDataBase database;

    [Header("Prefab")]
    [SerializeField] private GameObject unlockItemPrefab;
    [SerializeField] private GameObject unlockPrerequireText;

    [Header("Category Parents")]
    [SerializeField] private Transform patchParent;
    [SerializeField] private Transform goalParent;
    [SerializeField] private Transform operationParent;

    [SerializeField] private Transform PreRequireParent;

    [Header("Inspector")]
    [SerializeField] private UnlockItemUI currentItem;

    [SerializeField] private TMP_Text unlockName;
    [SerializeField] private TMP_Text unlockDescription;
    [SerializeField] private TMP_Text unlockCost;
    [SerializeField] private Button UnlockButton;

    public Sprite UnlockSprite;

    void Start()
    {
        RegisterItems();
        UnlockButton.onClick.AddListener(UnlockItem);
    }

    public void Refresh()
    {
        foreach (UnlockCategory category in Enum.GetValues(typeof(UnlockCategory)))
        {
            foreach (Transform child in GetParent(category))
            {
                child.GetComponent<UnlockItemUI>().Refresh();
            }
        }
    }

    private void RegisterItems()
    {
        foreach (UnlockCategory category in Enum.GetValues(typeof(UnlockCategory)))
        {
            foreach (Transform child in GetParent(category))
            {
                child.GetComponent<UnlockItemUI>().Register(this);
            }
        }
    }

    private Transform GetParent(UnlockCategory category)
    {
        return category switch
        {
            UnlockCategory.Patch => patchParent,
            UnlockCategory.Goal => goalParent,
            UnlockCategory.Operation => operationParent,
            _ => null,
        };
    }

    public void SetInspector(UnlockItemUI item)
    {
        currentItem = item;
        unlockName.text = item.UnlockData.unlockName;
        unlockDescription.text = item.UnlockData.description;
        unlockCost.text = item.UnlockData.costResource.ToString();

        foreach (Transform child in PreRequireParent)
        {
            Destroy(child.gameObject);
        }

        if (item.UnlockData.prerequisites.Count() > 0)
        {
            foreach (UnlockData pre in item.UnlockData.prerequisites)
            {
                TMP_Text text = Instantiate(unlockPrerequireText, PreRequireParent).GetComponent<TMP_Text>();
                text.text = pre.unlockName;
            }
        }
    }

    private void UnlockItem()
    {
        currentItem.Unlock();
    }
}
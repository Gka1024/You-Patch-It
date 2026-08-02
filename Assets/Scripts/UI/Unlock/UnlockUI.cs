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

    [Header("Category Parents")]
    [SerializeField] private Transform patchParent;
    [SerializeField] private Transform goalParent;
    [SerializeField] private Transform informationParent;
    [SerializeField] private Transform operationParent;

    [Header("Inspector")]
    [SerializeField] private UnlockItemUI currentItem;

    [SerializeField] private TMP_Text unlockName;
    [SerializeField] private TMP_Text unlockDescription;
    [SerializeField] private TMP_Text unlockCost;
    [SerializeField] private Button UnlockButton;

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
    }

    private void UnlockItem()
    {
        currentItem.Unlock();
    }
}
using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class EncounterPopupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text Title;
    [SerializeField] private TMP_Text Description;
    [SerializeField] private Button ConfirmButton;

    [SerializeField] private GameObject EncounterContentParent;
    [SerializeField] private GameObject EncounterContentTextPrefab;

    void Awake()
    {
        ConfirmButton.onClick.AddListener(Comfirm);
    }

    public void Initialize(Encounter encounter)
    {
        Title.text = encounter.Name;
        Description.text = encounter.Description;
        SetContents(encounter);
    }

    private void SetContents(Encounter encounter)
    {
        foreach (EncounterResource resource in encounter.EncounterResources)
        {
            TMP_Text text = Instantiate(EncounterContentTextPrefab, EncounterContentParent.transform).GetComponent<TextMeshProUGUI>();
            text.text = GetEncounterText(resource);
        }
    }

    private string GetEncounterText(EncounterResource resource)
    {
        string text = "";

        switch (resource.goodsType)
        {
            case GoodsType.Trust:
                text += "신뢰도";
                break;
            case GoodsType.DevelopResource:
                text += "리소스";
                break;
            case GoodsType.Player:
                text += "플레이어 수";
                break;
            case GoodsType.Money:
                text += "운영자금";
                break;
        }

        text += $" : {(resource.isNegative ? "-" : "")}{resource.value}";

        if (resource.valueFormat == ValueFormat.Percent)
        {
            text += "%";
        }

        switch (resource.valuePeriod)
        {
            case ValuePeriod.CurrentSeason:
                text += " (이번 시즌)";
                break;
            case ValuePeriod.Total:
                text += " (전체)";
                break;
        }

        return text;
    }

    private void Comfirm()
    {
        this.gameObject.SetActive(false);
    }
}

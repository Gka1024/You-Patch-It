using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncounterPopupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text Title;
    [SerializeField] private TMP_Text Description;
    [SerializeField] private Button ConfirmButton;

    void Awake()
    {
        ConfirmButton.onClick.AddListener(Comfirm);
    }

    public void Initialize(Encounter encounter)
    {
        Title.text = encounter.Name;
        Description.text = encounter.Description;
    }

    private void Comfirm()
    {
        this.gameObject.SetActive(false);
    }
}

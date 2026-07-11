using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectorUI : MonoBehaviour
{
    public static InspectorUI Instance;

    private RuntimeCharacter currentCharacter;

    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button applyPatchButton;

    [Header("Stats")]
    [SerializeField] private Button StatsButton;
    [SerializeField] private GameObject Stats;
    [SerializeField] private GameObject[] StatRows;

    [Header("Analyses")]
    [SerializeField] private Button AnalysisButton;
    [SerializeField] private GameObject Analysis;
    [SerializeField] private GameObject[] AnalysisRows;

    [Header("Winrate")]
    [SerializeField] private Button WinrateButton;
    [SerializeField] private GameObject Winrate;


    private void Awake()
    {
        Instance = this;
        applyPatchButton.onClick.AddListener(ApplyPatch);
        StatsButton.onClick.AddListener(ShowStats);
        WinrateButton.onClick.AddListener(ShowHistorys);
        AnalysisButton.onClick.AddListener(ShowAnalysis);
    }

    public void Show(RuntimeCharacter character)
    {
        currentCharacter = character;
        InitializeStats();
        InitializeAnalysis();
        InitializeWinrate();
        Refresh();
    }

    public void Refresh()
    {
        if (currentCharacter == null)
        {
            return;
        }

        nameText.text = currentCharacter.OriginCharacter.characterName;
    }

    private void InitializeStats()
    {
        foreach (var row in StatRows)
        {
            row.SetActive(true);
            row.GetComponent<InspectorRowUI>().Initialize(currentCharacter);
        }
    }

    private void InitializeAnalysis()
    {
        foreach (var row in AnalysisRows)
        {
            row.GetComponent<InspectorAnalysisRowUI>().Initialize(currentCharacter);
        }
    }

    private void InitializeWinrate()
    {
        Winrate.GetComponent<InspectorWinrateUI>().Initialize(currentCharacter);
    }

    private void ApplyPatch()
    {
        if (currentCharacter == null)
            return;

        List<CharacterPatch> patches = new();

        foreach (GameObject row in StatRows)
        {
            InspectorRowUI rowUI = row.GetComponent<InspectorRowUI>();
            patches.Add(rowUI.GetPatch());
        }

        currentCharacter.Patch(patches);

        Refresh();
        InitializeStats();
    }

    public void ShowStats()
    {
        HideInspector();
        Stats.SetActive(true);
    }

    public void ShowHistorys()
    {
        HideInspector();
        Winrate.SetActive(true);
    }

    public void ShowAnalysis()
    {
        HideInspector();
        Analysis.SetActive(true);
    }

    private void HideInspector()
    {
        Stats.SetActive(false);
        Winrate.SetActive(false);
        Analysis.SetActive(false);
    }
}
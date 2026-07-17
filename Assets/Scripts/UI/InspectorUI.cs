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
    [SerializeField] private Button undoButton;

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

    [Header("Bellow")]
    [SerializeField] private GameObject patchReason;
    [SerializeField] private Button patchConfirmButton;
    [SerializeField] private Button simulateButton;

    private void Awake()
    {
        Instance = this;
        applyPatchButton.onClick.AddListener(ShowPatchReason);
        StatsButton.onClick.AddListener(ShowStats);
        WinrateButton.onClick.AddListener(ShowHistorys);
        AnalysisButton.onClick.AddListener(ShowAnalysis);
        patchConfirmButton.onClick.AddListener(ApplyPatch);
        simulateButton.onClick.AddListener(Refresh);
    }

    private void Start()
    {
        PatchManager.Instance.OnPatchApplied += Refresh;
        PatchManager.Instance.OnPatchUndone += Refresh;
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
        if (currentCharacter == null) return;

        nameText.text = currentCharacter.OriginCharacter.characterName;
        InitializeStats();
        InitializeAnalysis();
        InitializeWinrate();
    }

    public void Refresh(PatchRecord record)
    {
        if (!ReferenceEquals(currentCharacter, record.Character)) return;

        InitializeStats();
    }

    public void InitializeStats()
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

    private void ShowPatchReason()
    {
        if (currentCharacter == null) return;
        patchReason.SetActive(true);
        patchConfirmButton.interactable = false;
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

        List<PatchReason> reasons = patchReason.GetComponent<PatchReasonPopupUI>().GetPatchReasons();

        PatchManager.Instance.ApplyPatch(currentCharacter, patches, reasons);

        InitializeStats();
        patchReason.SetActive(false);
        Refresh();
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
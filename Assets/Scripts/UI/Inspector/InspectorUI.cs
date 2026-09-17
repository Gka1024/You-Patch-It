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
    private readonly Dictionary<Transform, int> originalSiblingIndexes = new();

    [Header("Image")]
    [SerializeField] private Image roleImage;

    [SerializeField] private Sprite warriorSprite;
    [SerializeField] private Sprite rangedSprite;
    [SerializeField] private Sprite mageSprite;
    [SerializeField] private Sprite assassinSprite;
    [SerializeField] private Sprite tankSprite;
    [SerializeField] private Sprite supportSprite;

    [Header("Stats")]
    [SerializeField] private Button StatsButton;
    [SerializeField] private GameObject Stats;
    [SerializeField] private GameObject[] StatRows;
    [SerializeField] private Transform NormalStatRows;
    [SerializeField] private Transform SpecialStatRows;

    [SerializeField] private TMP_Dropdown StatDropDown;
    private readonly List<CharacterStatType> dropdownStatTypes = new();


    [Header("Analyses")]
    [SerializeField] private Button AnalysisButton;
    [SerializeField] private GameObject Analysis;
    [SerializeField] private GameObject[] AnalysisRows;

    [Header("Winrate")]
    [SerializeField] private Button WinrateButton;
    [SerializeField] private GameObject Winrate;

    [Header("Bellow")]
    [SerializeField] private PatchReasonPopupUI patchReason;
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
        CacheOriginalSiblingIndexes();
    }

    private void Start()
    {
        PatchManager.Instance.OnPatchApplied += RefreshOnPatched;
        PatchManager.Instance.OnPatchUndone += RefreshOnPatched;
    }

    // ==============================
    // Set Character
    // ==============================

    public void Showcharacter(RuntimeCharacter character)
    {
        currentCharacter = character;
        patchReason.Show(false);
        SetRoleImage(character.OriginCharacter.role);
        SetCharacterSkillDescription();
        InitializeStatDropDown();
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

    public void RefreshOnPatched(PatchRecord record)
    {
        if (!ReferenceEquals(currentCharacter, record.Character)) return;

        InitializeStats();
    }

    private void SetRoleImage(CharacterRole role)
    {
        roleImage.sprite = role switch
        {
            CharacterRole.Warrior => warriorSprite,
            CharacterRole.Ranged => rangedSprite,
            CharacterRole.Mage => mageSprite,
            CharacterRole.Assassin => assassinSprite,
            CharacterRole.Tank => tankSprite,
            CharacterRole.Support => supportSprite,
            _ => null
        };
    }

    private void SetCharacterSkillDescription()
    {
        UIManager.Instance.bottomDisplayUI.ShowDescription();
    }

    // ==============================
    // Initialize
    // ==============================


    private void CacheOriginalSiblingIndexes()
    {
        originalSiblingIndexes.Clear();

        foreach (GameObject row in StatRows)
        {
            originalSiblingIndexes[row.transform] = row.transform.GetSiblingIndex();
        }
    }

    public void InitializeStats()
    {
        ShowAllStatRows(false);
        ClearSpecialStatRows();

        foreach (var row in StatRows)
        {
            if (currentCharacter.OriginCharacter.defaultEditableStats.Contains(row.GetComponent<InspectorRowUI>().StatType))
            {
                row.SetActive(true);
                row.GetComponent<InspectorRowUI>().Initialize(currentCharacter);
            }

            if (currentCharacter.HasPatchedSpecialStat() == row.GetComponent<InspectorRowUI>().StatType)
            {
                ShowSpecificStats(row.GetComponent<InspectorRowUI>().StatType);
            }
        }
    }

    private void InitializeStatDropDown()
    {
        StatDropDown.onValueChanged.RemoveAllListeners();

        List<string> options = new() { "스탯 선택" };
        dropdownStatTypes.Clear();

        CharacterStatType? patchedStat = currentCharacter.HasPatchedSpecialStat();

        foreach (CharacterStatType type in System.Enum.GetValues(typeof(CharacterStatType)))
        {
            // 기본 수정 가능 스탯은 드롭다운에서 제외
            if (currentCharacter.OriginCharacter.defaultEditableStats.Contains(type))
                continue;

            options.Add(DisplayNameHelper.GetStatName(type));
            dropdownStatTypes.Add(type);
        }

        StatDropDown.ClearOptions();
        StatDropDown.AddOptions(options);

        // 이미 특수 스탯을 패치했다면 해당 스탯으로 고정
        if (patchedStat.HasValue)
        {
            int index = dropdownStatTypes.IndexOf(patchedStat.Value);

            if (index >= 0)
            {
                StatDropDown.value = index + 1;
                StatDropDown.interactable = false;
                ShowSpecificStats(patchedStat.Value);
            }
        }
        else
        {
            // 아직 특수 스탯을 패치하지 않았다면 선택 가능
            StatDropDown.value = 0;
            StatDropDown.interactable = true;
        }

        StatDropDown.RefreshShownValue();
        StatDropDown.onValueChanged.AddListener(OnStatChanged);
    }

    private void InitializeAnalysis()
    {
        foreach (var row in AnalysisRows)
        {
            row.GetComponent<InspectorStatisticRowUI>().Initialize(currentCharacter);
        }
    }

    private void InitializeWinrate()
    {
        Winrate.GetComponent<InspectorCombatAnalysisUI>().Initialize(currentCharacter);
    }


    // ==============================
    // Patch
    // ==============================

    private void ShowPatchReason()
    {
        if (currentCharacter == null) return;
        if (!CheckDelta()) return;

        patchReason.Show(true);
        patchConfirmButton.interactable = false;
        patchReason.ResourceLackAlert.SetActive(false);
    }

    private bool CheckDelta()
    {
        foreach (GameObject row in StatRows)
        {
            if (row.GetComponent<InspectorRowUI>().HasChange()) return true;
        }
        return false;
    }

    private void ApplyPatch()
    {
        if (currentCharacter == null)
            return;

        List<CharacterPatch> patches = new();
        bool hasChange = false;

        foreach (GameObject row in StatRows)
        {
            InspectorRowUI rowUI = row.GetComponent<InspectorRowUI>();

            if (!rowUI.HasChange())
                continue;

            hasChange = true;
            patches.Add(rowUI.GetPatch());
        }

        if (!hasChange)
        {
            patchReason.Show(false);
            return;
        }

        List<PatchReason> reasons = patchReason.GetComponent<PatchReasonPopupUI>().GetPatchReasons();

        if (PatchManager.Instance.ApplyPatch(currentCharacter, patches, reasons))
        {
            InitializeStats();
            patchReason.Show(false);

            foreach (GameObject row in StatRows)
            {
                InspectorRowUI rowUI = row.GetComponent<InspectorRowUI>();

                if (rowUI.HasChange() && !currentCharacter.OriginCharacter.defaultEditableStats.Contains(rowUI.StatType))
                {
                    currentCharacter.SetPatchedSpecialStat(rowUI.StatType);
                }
            }
        }

        Refresh();
    }

    // ==============================
    // Show / Hide
    // ==============================

    private void ShowAllStatRows(bool show)
    {
        foreach (var row in StatRows)
        {
            row.SetActive(show);
        }
    }

    private void ShowSpecificStats(CharacterStatType stat)
    {
        ClearSpecialStatRows();

        foreach (GameObject row in StatRows)
        {
            InspectorRowUI rowUI = row.GetComponent<InspectorRowUI>();

            if (rowUI.StatType != stat)
                continue;

            Debug.Log(DisplayNameHelper.GetStatName(rowUI.StatType));

            row.transform.SetParent(SpecialStatRows, false);
            row.transform.localPosition = Vector3.zero;
            row.SetActive(true);
            rowUI.Initialize(currentCharacter);
            break;
        }
    }

    private void ClearSpecialStatRows()
    {
        foreach (Transform child in SpecialStatRows)
        {
            child.SetParent(NormalStatRows, false);
            child.gameObject.GetComponent<InspectorRowUI>().ResetValue();
            child.gameObject.SetActive(false);
        }

        for (int i = 0; i < StatRows.Length; i++)
        {
            StatRows[i].transform.SetSiblingIndex(i);
        }
    }

    private void OnStatChanged(int index)
    {
        if (index == 0)
            return;

        int statIndex = index - 1;

        if (statIndex < 0 || statIndex >= dropdownStatTypes.Count)
            return;

        ShowSpecificStats(dropdownStatTypes[statIndex]);
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
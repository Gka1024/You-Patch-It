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
    [SerializeField] private Button WriteReasonButton;
    [SerializeField] private TMP_Text patchResourceText;
    [SerializeField] private GameObject PatchReasonUI;
    [SerializeField] private TMP_InputField PatchReasonInput;

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

    [Header("Analyses")]
    [SerializeField] private Button AnalysisButton;
    [SerializeField] private GameObject Analysis;
    [SerializeField] private GameObject[] AnalysisRows;

    [Header("Winrate")]
    [SerializeField] private Button WinrateButton;
    [SerializeField] private GameObject Winrate;

    [Header("Current Character")]

    [SerializeField] private int currentPatchResource = 0;
    private List<CharacterPatch> currentCharacterPatches = new();

    private void Awake()
    {
        Instance = this;

        applyPatchButton.onClick.AddListener(ApplyPatch);
        WriteReasonButton.onClick.AddListener(ShowPatchReason);
        StatsButton.onClick.AddListener(ShowStats);
        WinrateButton.onClick.AddListener(ShowHistorys);
        AnalysisButton.onClick.AddListener(ShowAnalysis);

        CacheOriginalSiblingIndexes();
    }

    private void Start()
    {
        PatchManager.Instance.OnPatchApplied += RefreshOnPatched;
        PatchManager.Instance.OnPatchUndone += RefreshOnPatched;
        UnlockManager.Instance.OnUnlockChanged += RefreshPatchResourceText;
    }

    private void RefreshPatchResourceText()
    {
        patchResourceText.text = $"패치 리소스 : {PatchManager.Instance.GetRequiredResource()}";
    }

    private void OnDestroy()
    {
        if (applyPatchButton != null)
            applyPatchButton.onClick.RemoveListener(ApplyPatch);

        if (WriteReasonButton != null)
            WriteReasonButton.onClick.RemoveListener(ShowPatchReason);

        if (StatsButton != null)
            StatsButton.onClick.RemoveListener(ShowStats);

        if (WinrateButton != null)
            WinrateButton.onClick.RemoveListener(ShowHistorys);

        if (AnalysisButton != null)
            AnalysisButton.onClick.RemoveListener(ShowAnalysis);

        if (PatchManager.Instance != null)
        {
            PatchManager.Instance.OnPatchApplied -= RefreshOnPatched;
            PatchManager.Instance.OnPatchUndone -= RefreshOnPatched;
        }

        if (Instance == this)
            Instance = null;
    }

    // ==============================
    // Set Character
    // ==============================

    public void Showcharacter(RuntimeCharacter character)
    {
        if (character == null)
            return;

        currentCharacter = character;
        currentCharacterPatches.Clear();

        SetRoleImage(character.OriginCharacter.role);
        SetCharacterSkillDescription();

        Refresh();
    }

    public void Refresh()
    {
        if (currentCharacter == null)
            return;

        nameText.text = currentCharacter.OriginCharacter.characterName;

        InitializeStats();
        InitializeAnalysis();
        InitializeWinrate();
    }

    public void RefreshOnPatched(PatchRecord record)
    {
        if (!ReferenceEquals(currentCharacter, record.Character))
            return;

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
        ShowAllStatRows(true);

        foreach (GameObject row in StatRows)
        {
            InspectorRowUI rowUI = row.GetComponent<InspectorRowUI>();

            rowUI.Initialize(this, currentCharacter);

            if (currentCharacter.OriginCharacter.recommendEditStats.Contains(rowUI.StatType))
            {
                rowUI.SetRecommended();
            }
        }
    }

    /*
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
                StatDropDown.value = 0;
                StatDropDown.interactable = true;
            }

            StatDropDown.RefreshShownValue();
            StatDropDown.onValueChanged.AddListener(OnStatChanged);
        }
    */
    private void InitializeAnalysis()
    {
        foreach (GameObject row in AnalysisRows)
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
        if (currentCharacter == null)
            return;

        PatchReasonUI.SetActive(true);
    }

    public void CheckDelta()
    {
        currentCharacterPatches.Clear();

        foreach (GameObject row in StatRows)
        {
            InspectorRowUI rowUI = row.GetComponent<InspectorRowUI>();

            if (rowUI.HasChange())
                currentCharacterPatches.Add(rowUI.GetPatch());
        }

        currentPatchResource = PatchManager.Instance.CalculateRequiredResource(currentCharacter, currentCharacterPatches);
        patchResourceText.text = $"패치 리소스 : {currentPatchResource}";
    }

    private void ApplyPatch()
    {
        if (currentCharacter == null)
            return;

        if (currentCharacterPatches.Count == 0)
        {
            PatchReasonUI.SetActive(false);
            return;
        }

        string description = PatchReasonInput.text.Trim();

        if (!PatchManager.Instance.ApplyPatch(currentCharacter, currentCharacterPatches, description))
            return;

        currentCharacterPatches.Clear();
        PatchReasonUI.SetActive(false);
        PatchReasonInput.text = "";

        // 드롭다운 및 스탯 UI 갱신
        Refresh();
    }

    // ==============================
    // Show / Hide
    // ==============================

    private void ShowAllStatRows(bool show)
    {
        foreach (GameObject row in StatRows)
        {
            row.SetActive(show);
        }
    }
    /*
      private void ShowSpecificStats(CharacterStatType stat)
      {

          foreach (GameObject row in StatRows)
          {
              InspectorRowUI rowUI = row.GetComponent<InspectorRowUI>();

              if (rowUI.StatType != stat)
                  continue;

              RectTransform rect = row.GetComponent<RectTransform>();

              row.transform.SetParent(SpecialStatRows, false);
              rect.anchoredPosition = Vector2.zero;

              row.SetActive(true);
              rowUI.Initialize(currentCharacter);

              break;
          }
      }

          private void ClearSpecialStatRows()
          {
              // 자식 오브젝트를 하나씩 이동
              // foreach 중 부모가 변경되는 문제 방지
              while (SpecialStatRows.childCount > 0)
              {
                  Transform child = SpecialStatRows.GetChild(0);

                  child.SetParent(NormalStatRows, false);

                  InspectorRowUI rowUI = child.GetComponent<InspectorRowUI>();
                  rowUI.ResetValue();

                  child.gameObject.SetActive(false);
              }

              // 기존 계층 구조 복구
              foreach (GameObject row in StatRows)
              {
                  Transform rowTransform = row.transform;

                  if (originalSiblingIndexes.TryGetValue(rowTransform, out int siblingIndex))
                  {
                      rowTransform.SetSiblingIndex(siblingIndex);
                  }
              }
          }

    private void OnStatChanged(int index)
    {
        if (index == 0)
            return;

        int statIndex = index - 1;

        if (statIndex < 0 || statIndex >= dropdownStatTypes.Count)
            return;

    }
      */
    public void ShowPatchResourceLackAlert(bool show)
    {
        // TODO
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
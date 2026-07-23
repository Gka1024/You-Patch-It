using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PatchManager : MonoBehaviour
{
    public static PatchManager Instance { get; private set; }

    public event Action<PatchRecord> OnPatchApplied;
    public event Action<PatchRecord> OnPatchUndone;

    /// <summary>
    /// 현재 적용되어 있는 패치
    /// </summary>
    private readonly List<PatchRecord> appliedPatches = new();
    public IReadOnlyList<PatchRecord> AppliedPatches => appliedPatches;

    /// <summary>
    /// Undo용
    /// </summary>
    private readonly Stack<PatchRecord> undoStack = new();
    public bool CanUndo => undoStack.Count > 0;

    private const int PATCH_REQUIRE_RESOURCE = 10;

    private void Awake()
    {
        Instance = this;
    }

    //====================================================
    // Patch
    //====================================================

    public void StartPatch()
    {
        appliedPatches.Clear();
        undoStack.Clear();

        Debug.Log("Patch Start");
    }

    public void ConfirmPatch()
    {
        SeasonManager.Instance.FinishPatch();
    }

    public void ApplyPatch(
        RuntimeCharacter character,
        List<CharacterPatch> patches,
        List<PatchReason> reasons)
    {
        if (!ResourceManager.Instance.SpendDevelopResource(PATCH_REQUIRE_RESOURCE))
            return;

        if (character == null)
            return;

        if (patches == null || patches.Count == 0)
            return;

        RuntimeCharacterSnapshot before =
            new RuntimeCharacterSnapshot(character);

        character.Patch(patches);

        RuntimeCharacterSnapshot after =
            new RuntimeCharacterSnapshot(character);

        PatchRecord record =
            new PatchRecord(character, before, after, patches, reasons);

        appliedPatches.Add(record);
        undoStack.Push(record);

        OnPatchApplied?.Invoke(record);
    }

    public void Undo()
    {
        if (!CanUndo)
            return;

        PatchRecord record = undoStack.Pop();

        record.Character.RestoreSnapshot(record.Before);

        appliedPatches.Remove(record);

        OnPatchUndone?.Invoke(record);
    }

    public void ResetHistory()
    {
        appliedPatches.Clear();
        undoStack.Clear();
    }

    //====================================================
    // Goal Helper
    //====================================================

    /// <summary>
    /// 해당 스탯이 원본과 달라졌는가
    /// </summary>
    public bool IsStatModified(RuntimeCharacter character, CharacterStatType stat)
    {
        RuntimeStat runtimeStat = character.Stats[stat];

        return !Mathf.Approximately(runtimeStat.BaseValue, runtimeStat.CurrentValue);
    }

    /// <summary>
    /// 현재 수정된 능력치 개수
    /// </summary>
    public int GetModifiedStatCount(RuntimeCharacter character)
    {
        int count = 0;

        foreach (CharacterStatType stat in Enum.GetValues(typeof(CharacterStatType)))
        {
            if (IsStatModified(character, stat))
                count++;
        }

        return count;
    }

    /// <summary>
    /// 현재 수정된 능력치 목록
    /// </summary>
    public List<CharacterStatType> GetModifiedStats(RuntimeCharacter character)
    {
        List<CharacterStatType> result = new();

        foreach (CharacterStatType stat in Enum.GetValues(typeof(CharacterStatType)))
        {
            if (IsStatModified(character, stat))
                result.Add(stat);
        }

        return result;
    }

    /// <summary>
    /// 모든 캐릭터가 해당 스탯을 수정하지 않았는가
    /// </summary>
    public bool IsStatUntouched(CharacterStatType stat)
    {
        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            if (IsStatModified(character, stat))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 모든 캐릭터가 허용된 능력치만 수정했는가
    /// </summary>
    public bool OnlyModified(params CharacterStatType[] allowed)
    {
        HashSet<CharacterStatType> allow = new(allowed);

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            foreach (CharacterStatType stat in GetModifiedStats(character))
            {
                if (!allow.Contains(stat))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 모든 캐릭터가 최대 n개의 능력치만 수정했는가
    /// </summary>
    public bool MaxModifiedStatCount(int maxCount)
    {
        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            if (GetModifiedStatCount(character) > maxCount)
                return false;
        }

        return true;
    }
}

public enum PatchReason
{
    None,
    HighWinrate,
    LowWinrate,
    HighPickrate,
    LowPickrate,
    HighBanrate,
    LowBanrate,
    MetaDiversity,
    UserFeedBack,
    InternalTest,
    Other,
}

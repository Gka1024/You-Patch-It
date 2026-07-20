using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Undo용
    /// </summary>
    private readonly Stack<PatchRecord> undoStack = new();

    public IReadOnlyList<PatchRecord> AppliedPatches => appliedPatches;

    public bool CanUndo => undoStack.Count > 0;

    private void Awake()
    {
        Instance = this;
    }

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

    public void ApplyPatch(RuntimeCharacter character, List<CharacterPatch> patches, List<PatchReason> reasons)
    {
        if (character == null)
            return;

        if (patches == null || patches.Count == 0)
            return;

        RuntimeCharacterSnapshot before = new RuntimeCharacterSnapshot(character);

        character.Patch(patches);

        RuntimeCharacterSnapshot after = new RuntimeCharacterSnapshot(character);

        PatchRecord record = new PatchRecord(character, before, after, patches, reasons);

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
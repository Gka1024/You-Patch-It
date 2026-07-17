using System.Collections.Generic;

public class PatchRecord
{
    public RuntimeCharacter Character { get; }

    public RuntimeCharacterSnapshot Before { get; }

    public RuntimeCharacterSnapshot After { get; }

    public IReadOnlyList<CharacterPatch> Patches => patches;

    public IReadOnlyList<PatchReason> Reasons => reasons;

    private readonly List<CharacterPatch> patches;

    private readonly List<PatchReason> reasons;

    public PatchRecord(
        RuntimeCharacter character,
        RuntimeCharacterSnapshot before,
        RuntimeCharacterSnapshot after,
        List<CharacterPatch> patches,
        List<PatchReason> reasons)
    {
        Character = character;

        Before = before;

        After = after;

        this.patches = new List<CharacterPatch>(patches);

        this.reasons = new List<PatchReason>(reasons);
    }
}
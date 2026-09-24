using System.Collections.Generic;

public class PatchRecord
{
    public RuntimeCharacter Character { get; }

    public RuntimeCharacterSnapshot Before { get; }
    public RuntimeCharacterSnapshot After { get; }

    public IReadOnlyList<CharacterPatch> Patches => patches;
    public string PatchDescription { get; }

    private readonly List<CharacterPatch> patches;

    public PatchRecord(RuntimeCharacter character, RuntimeCharacterSnapshot before, RuntimeCharacterSnapshot after, List<CharacterPatch> patches, string patchDescription)
    {
        Character = character;

        Before = before;
        After = after;

        this.patches = new(patches);
        PatchDescription = patchDescription;
    }
}
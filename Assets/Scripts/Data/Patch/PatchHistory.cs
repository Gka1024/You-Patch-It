using System.Collections.Generic;
using System.Linq;

public class PatchHistory
{
    public RuntimeCharacter Character { get; }

    public int Season { get; }
    public int SubSeason { get; }

    public float Winrate { get; }
    public float Pickrate { get; }

    public IReadOnlyList<PatchRecord> Records => records;

    private readonly List<PatchRecord> records;

    public PatchHistory(RuntimeCharacter character, int season, int subSeason, float winrate, float pickrate, List<PatchRecord> records)
    {
        Character = character;

        Season = season;
        SubSeason = subSeason;

        Winrate = winrate;
        Pickrate = pickrate;

        this.records = records;
    }

    public bool TryGetStatPatch(CharacterStatType stat, out float before, out float after)
    {
        before = 0;
        after = 0;

        PatchRecord first =
            records.FirstOrDefault(x =>
                x.Patches.Any(p => p.StatType == stat));

        if (first == null)
            return false;

        PatchRecord last =
            records.Last(x =>
                x.Patches.Any(p => p.StatType == stat));

        before = first.Before.Stats[stat].CurrentValue;
        after = last.After.Stats[stat].CurrentValue;

        return true;
    }

    public IEnumerable<PatchReason> GetReasons()
    {
        return records
            .SelectMany(x => x.Reasons)
            .Distinct();
    }
}
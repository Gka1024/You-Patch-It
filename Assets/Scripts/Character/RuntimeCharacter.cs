using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RuntimeCharacter
{
    public Character OriginCharacter { get; }
    public Dictionary<CharacterStatType, RuntimeStat> Stats { get; } = new();
    public event Action OnStatChanged;

    public RuntimeCharacter(Character character)
    {
        OriginCharacter = character;
        foreach (var stat in character.stats)
        {
            Stats.Add(stat.statType, new RuntimeStat(stat.value));
        }
    }

    public float GetStat(CharacterStatType stat)
    {
        return Stats[stat].CurrentValue;
    }

    public void Patch(List<CharacterPatch> patches)
    {
        foreach(var patch in patches)
        {
            Patch(patch.StatType, patch.After);
        }
    }

    public void Patch(CharacterStatType stat, float value)
    {
        Stats[stat].SetValue(value);

        OnStatChanged?.Invoke();
    }

    public RuntimeCharacterSnapshot CreateSnapshot()
    {
        return new RuntimeCharacterSnapshot(this);
    }

    public void RestoreSnapshot(RuntimeCharacterSnapshot snapshot)
    {
        foreach (var pair in snapshot.Stats)
        {
            Stats[pair.Key].RestoreSnapshot(pair.Value);
        }

        OnStatChanged?.Invoke();
    }

    public void Reset()
    {
        foreach (var stat in Stats.Values)
        {
            stat.Reset();
        }
    }
}

public class RuntimeCharacterSnapshot
{
    public Dictionary<CharacterStatType, RuntimeStatSnapshot> Stats { get; }

    public RuntimeCharacterSnapshot(RuntimeCharacter character)
    {
        Stats = new();

        foreach (var pair in character.Stats)
        {
            Stats.Add(pair.Key, pair.Value.CreateSnapshot());
        }
    }
}
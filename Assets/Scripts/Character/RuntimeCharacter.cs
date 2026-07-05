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
        foreach(var stat in character.stats)
        {
            Stats.Add(stat.statType, new RuntimeStat(stat.value));
        }
    }

    public float GetStat(CharacterStatType stat)
    {
        return Stats[stat].CurrentValue;
    }

    public void Patch(CharacterStatType stat, float delta, string reason)
    {
        Stats[stat].AddModifier(new StatModifier(reason, delta));
        OnStatChanged?.Invoke();
    }

    public void Reset()
    {
        foreach(var stat in Stats.Values)
        {
            stat.Reset();
        }
    }
}
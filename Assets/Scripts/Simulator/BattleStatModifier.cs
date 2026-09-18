public class BattleStatModifier
{
    public CharacterStatType statType;
    public BattleStatModifierType modifierType;
    public float value;

    public float remainingTicks;

    public object Source;

    public BattleStatModifier(CharacterStatType statType, BattleStatModifierType statModifier, float value, float durationTicks, object Source = null)
    {
        this.statType = statType;
        this.modifierType = statModifier;
        this.value = value;
        this.remainingTicks = durationTicks;
        this.Source = Source;
    }
}

public enum BattleStatModifierType
{
    Percent,
    Flat
}
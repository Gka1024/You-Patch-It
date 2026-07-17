public class RuntimeStat
{
    public float BaseValue { get; }

    public float CurrentValue { get; private set; }

    public RuntimeStat(float baseValue)
    {
        BaseValue = baseValue;
        CurrentValue = baseValue;
    }

    public void SetValue(float value)
    {
        CurrentValue = value;
    }

    public RuntimeStatSnapshot CreateSnapshot()
    {
        return new(CurrentValue);
    }

    public void RestoreSnapshot(RuntimeStatSnapshot snapshot)
    {
        CurrentValue = snapshot.CurrentValue;
    }

    public void Reset()
    {
        CurrentValue = BaseValue;
    }
}

public class RuntimeStatSnapshot
{
    public float CurrentValue { get; }


    public RuntimeStatSnapshot(float currentValue)
    {
        CurrentValue = currentValue;
        
    }
}
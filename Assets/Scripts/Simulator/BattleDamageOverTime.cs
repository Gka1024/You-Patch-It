using System;

public class BattleDamageOverTime
{
    private readonly BattleCharacter source;
    private readonly BattleCharacter target;
    private readonly float value;
    private readonly float duration;
    private readonly float tickInterval;
    private readonly Action<BattleCharacter, BattleCharacter, float> effect;

    private float remainingTime;
    private float tickTimer;

    public BattleDamageOverTime(BattleCharacter source, BattleCharacter target, float value, float duration, float tickInterval, Action<BattleCharacter, BattleCharacter, float> effect)
    {
        this.source = source;
        this.target = target;
        this.value = value;
        this.duration = duration;
        this.tickInterval = tickInterval;
        this.effect = effect;

        remainingTime = duration;
        tickTimer = 0f;
    }

    public bool Tick(float tick)
    {
        if (target.IsDead)
            return true;

        remainingTime -= tick;
        tickTimer -= tick;

        if (tickTimer <= 0f)
        {
            UnityEngine.Debug.Log("Tick");
            effect(source, target, value);
            tickTimer += tickInterval;
        }

        return remainingTime <= 0f;
    }
}
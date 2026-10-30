using System;
using System.Collections.Generic;

public abstract class BattleSkillInstance
{
    protected readonly BattleCharacter self;
    protected readonly List<BattleCharacter> allies;
    protected readonly List<BattleCharacter> enemies;
    protected readonly float coefficient;
    protected readonly System.Random random;

    public bool IsFinished { get; private set; }

    private bool hasStarted;

    protected BattleSkillInstance(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        this.self = self;
        this.allies = allies;
        this.enemies = enemies;
        this.coefficient = coefficient;
        this.random = random;
    }

    public void Start()
    {
        if (hasStarted || IsFinished)
            return;

        hasStarted = true;
        OnStart();
    }

    public void Tick(float tick)
    {
        if (!hasStarted || IsFinished)
            return;

        OnTick(tick);
    }

    public void Cancel()
    {
        if (IsFinished)
            return;

        IsFinished = true;
        OnCancel();
    }

    protected void Finish()
    {
        if (IsFinished)
            return;

        IsFinished = true;
        OnFinish();
    }

    protected virtual void OnStart() { }

    protected abstract void OnTick(float tick);

    protected virtual void OnCancel() { }

    protected virtual void OnFinish() { }
}
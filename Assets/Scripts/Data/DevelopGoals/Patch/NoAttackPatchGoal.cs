public class NoAttackPatchGoal : DeveloperGoal
{
    public override string Title => "BANANATECH";

    public override string Description =>
        "공격력을 수정하지 않고 목표를 달성하세요.";

    public NoAttackPatchGoal(GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
    
    }

    protected override bool CheckCompleted()
    {
        return PatchManager.Instance.IsStatUntouched(CharacterStatType.Attack);
    }

    public override float GetCurrentProgress()
    {
        return CheckCompleted() ? 1f : 0f;
    }
}
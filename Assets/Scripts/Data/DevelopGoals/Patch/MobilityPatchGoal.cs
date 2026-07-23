public class MobilityPatchGoal : DeveloperGoal
{
    public override string Title => "기동성 패치";

    public override string Description =>
        "이동속도와 공격속도만 수정하여 목표를 달성하세요.";

    public MobilityPatchGoal(GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        
    }

    protected override bool CheckCompleted()
    {
        return PatchManager.Instance.OnlyModified(
            CharacterStatType.MoveSpeed,
            CharacterStatType.AttackSpeed);
    }

    public override float GetCurrentProgress()
    {
        return CheckCompleted() ? 1f : 0f;
    }
}
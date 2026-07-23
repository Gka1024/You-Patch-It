using UnityEngine;

public class PatchCountGoal : DeveloperGoal
{
    private readonly int maxPatchCount;

    public override string Title => "최소한의 수정";
    public override string Description =>
        $"이번 시즌 패치를 {maxPatchCount}회 이하만 사용하세요.";

    public PatchCountGoal(int maxPatchCount, GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        this.maxPatchCount = maxPatchCount;
    }

    protected override bool CheckCompleted()
    {
        return PatchManager.Instance.AppliedPatches.Count <= maxPatchCount;
    }

    public override float GetCurrentProgress()
    {
        int current = PatchManager.Instance.AppliedPatches.Count;

        if (current <= maxPatchCount)
            return 1f;

        return Mathf.Clamp01((float)maxPatchCount / current);
    }
}
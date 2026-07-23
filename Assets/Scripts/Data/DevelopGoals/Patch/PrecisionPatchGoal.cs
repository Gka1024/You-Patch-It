using UnityEngine;

public class PrecisionPatchGoal : DeveloperGoal
{
    public override string Title => "정밀 패치";

    public override string Description =>
        "캐릭터당 최대 2개의 능력치만 수정하세요.";

    public PrecisionPatchGoal(GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
    }

    protected override bool CheckCompleted()
    {
        return PatchManager.Instance.MaxModifiedStatCount(2);
    }

    public override float GetCurrentProgress()
    {
        int max = 0;

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            max = Mathf.Max(
                max,
                PatchManager.Instance.GetModifiedStatCount(character));
        }

        if (max <= 2)
            return 1f;

        return Mathf.Clamp01(2f / max);
    }
}
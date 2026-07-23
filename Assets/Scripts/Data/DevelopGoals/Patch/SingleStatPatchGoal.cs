using UnityEngine;

public class SingleStatPatchGoal : DeveloperGoal
{
    public override string Title => "전문가의 손길";

    public override string Description =>
        "각 캐릭터는 하나의 능력치만 수정할 수 있습니다.";

    public SingleStatPatchGoal(GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
    }

    protected override bool CheckCompleted()
    {
        return PatchManager.Instance.MaxModifiedStatCount(1);
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

        if (max <= 1)
            return 1f;

        return Mathf.Clamp01(1f / max);
    }
}
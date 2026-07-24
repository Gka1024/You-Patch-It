using System.Linq;
using UnityEngine;

public class BottomToTopGoal : DeveloperGoal
{
    private readonly RuntimeCharacter targetCharacter;
    private readonly int rank;

    public override string Title => "아메리칸 드림";

    public override string Description =>
        $"지난 시즌 최하위였던 {targetCharacter.OriginCharacter.characterName}을(를) 이번 시즌 승률 상위 {rank}위 안에 올리세요.";

    public BottomToTopGoal(RuntimeCharacter character, int rank, GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        this.targetCharacter = character;
        this.rank = rank;
    }

    protected override bool CheckCompleted()
    {
        return AnalysisManager.Instance.GetRank(    
            targetCharacter,
            AnalysisItem.Winrate,
            false) <= 4;
    }

    public override float GetCurrentProgress()
    {
        int rank = AnalysisManager.Instance.GetRank(
            targetCharacter,
            AnalysisItem.Winrate,
            false);

        if (rank <= 4)
            return 1f;

        int total =
            RuntimeCharacterManager.Instance.GetAllCharacters().ToList().Count;

        return Mathf.Clamp01(
            1f - ((float)(rank - 4) / Mathf.Max(1, total - 4)));
    }
}
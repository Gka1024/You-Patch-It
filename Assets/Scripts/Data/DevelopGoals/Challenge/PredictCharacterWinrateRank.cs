public class PredictCharacterWinrateRank : DeveloperGoal
{
    private readonly Character character;
    private readonly int rank;

    public override string Title => "노스트라다무스";
    public override string Description =>
    $"{character.characterName}의 승률 랭킹을 {rank}위로 만드세요.";

    public PredictCharacterWinrateRank(Character character, int rank, GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        this.character = character;
        this.rank = rank;
    }

    protected override bool CheckCompleted()
    {
        return AnalysisManager.Instance.GetRank(RuntimeCharacterManager.Instance.GetRuntimeCharacter(character.id), AnalysisItem.Winrate, false) == rank;
    }

    public override float GetCurrentProgress()
    {
        return CheckCompleted() ? 1 : 0;
    }
}
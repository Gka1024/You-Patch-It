public class SpecificCharacterWinrateGoal : DeveloperGoal
{
    private readonly float minWinrate;
    private readonly float maxWinrate;
    private RuntimeCharacter character;

    public override string Title => $"{character.OriginCharacter.characterName}가 좋아요";
    public override string Description =>
    $"{character.OriginCharacter.characterName}의 승률을 {minWinrate}% ~ {maxWinrate}%로 맞추세요.";

    public SpecificCharacterWinrateGoal(float minWinrate, float maxWinrate, GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        this.minWinrate = minWinrate;
        this.maxWinrate = maxWinrate;
    }

    public override void Refresh()
    {
        character = RuntimeCharacterManager.Instance.GetRandomCharacter();
    }

    protected override bool CheckCompleted()
    {
        float winrate = AnalysisManager.Instance.GetAnalysis(character, AnalysisItem.Winrate).CurrentValue;
        return 60 >= winrate && winrate >= 40;
    }

    public override float GetCurrentProgress()
    {
        return CheckCompleted() ? 1 : 0;
    }
}
public class SpecificCharacterWinrateGoal : DeveloperGoal
{
    private readonly float minWinrate;
    private readonly float maxWinrate;
    private Character character;

    public override string Title => $"{character.characterName}가 좋아요";
    public override string Description =>
    $"{character.characterName}의 승률을 {minWinrate}% ~ {maxWinrate}%로 맞추세요.";

    public SpecificCharacterWinrateGoal(float minWinrate, float maxWinrate,  GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        this.minWinrate = minWinrate;
        this.maxWinrate = maxWinrate;
    }

    public override void Refresh()
    {
        character = RuntimeCharacterManager.Instance.GetRandomCharacter().OriginCharacter;
    }

    protected override bool CheckCompleted()
    {
        return AnalysisManager.Instance.GetMaxValue(AnalysisItem.Pickrate, false) < 20f;
    }

    public override float GetCurrentProgress()
    {
        return CheckCompleted() ? 1 : 0;
    }
}
public class SpecificCharacterWinrateGoal : DeveloperGoal
{
    private readonly float minWinrate;
    private readonly float maxWinrate;
    private readonly Character character;

    public override string Title => $"{character.characterName}가 좋아요";
    public override string Description =>
    $"{character.characterName}의 승률을 {minWinrate}% ~ {maxWinrate}%로 맞추세요.";

    public SpecificCharacterWinrateGoal(float minWinrate, float maxWinrate, Character character)
    {
        this.minWinrate = minWinrate;
        this.maxWinrate = maxWinrate;
        this.character = character;
    }

    protected override bool CheckCompleted()
    {
        return AnalysisManager.Instance.GetMaxValue(AnalysisItem.Pickrate, false) < 20f;
    }
}
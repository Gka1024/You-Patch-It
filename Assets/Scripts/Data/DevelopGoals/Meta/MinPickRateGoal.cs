using System.Linq;

public class MinPickRateGoal : DeveloperGoal
{
    private readonly float minPickRate;

    public override string Title => "평등 신성화";
    public override string Description =>
    $"가장 픽률이 낮은 캐릭터의 픽률을 {minPickRate}% 이상으로 유지하세요.";

    public MinPickRateGoal(float min, GoalDifficulty difficulty, GoalType type) : base(difficulty, type)
    {
        this.minPickRate = min;
    }

    protected override bool CheckCompleted()
    {
        foreach(RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters().ToList())
        {
            if(AnalysisManager.Instance.GetPickRate(character) < minPickRate)
            {
                return false;
            }
        }

        return true;
    }

    public override float GetCurrentProgress()
    {
        return CheckCompleted() ? 1 : 0;
    }
}
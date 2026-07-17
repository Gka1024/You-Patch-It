public class BattleResult
{
    public RuntimeCharacter winner;
    public RuntimeCharacter loser;
    public bool isDraw;
    public float battleTime;

    public BattleStatistics statistics;

    public BattleResult(RuntimeCharacter winner, RuntimeCharacter loser, BattleStatistics statistics)
    {
        this.winner = winner;
        this.loser = loser;
        this.statistics = statistics;

    }
}

public class BattleStatistics
{
    public float battleDuration;
    public CharacterBattleStatistics Red { get; private set; }
    public CharacterBattleStatistics Blue { get; private set; }
    
    public void RegisterRed(CharacterBattleStatistics statistics)
    {
        Red = statistics;
    }

    public void RegisterBlue(CharacterBattleStatistics statistics)
    {
        Blue = statistics;
    }
}
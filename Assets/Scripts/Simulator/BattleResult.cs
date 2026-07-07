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
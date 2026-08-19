using System.Collections.Generic;

public class BattleResult
{
    public List<RuntimePlayer> redPlayer;
    public List<RuntimePlayer> bluePlayer;

    public List<RuntimeCharacter> winner;
    public List<RuntimeCharacter> loser;

    public bool isDraw;
    public float battleTime;

    public BattleStatistics statistics;

    public BattleResult(List<RuntimePlayer> redPlayer, List<RuntimePlayer> bluePlayer, List<RuntimeCharacter> winner, List<RuntimeCharacter> loser, BattleStatistics statistics)
    {
        this.redPlayer = redPlayer;
        this.bluePlayer = bluePlayer;

        this.winner = winner;
        this.loser = loser;

        this.statistics = statistics;
    }
}

public class BattleStatistics
{
    public float battleDuration;
    public List<CharacterBattleStatistics> Red { get; private set; } = new();
    public List<CharacterBattleStatistics> Blue { get; private set; } = new();

    public void RegisterRed(CharacterBattleStatistics statistics)
    {
        Red.Add(statistics);
    }

    public void RegisterBlue(CharacterBattleStatistics statistics)
    {
        Blue.Add(statistics);
    }
}
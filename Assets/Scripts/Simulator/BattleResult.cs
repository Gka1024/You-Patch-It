using System.Collections.Generic;

public class BattleResult
{
    public List<int> redCharacter;
    public List<int> blueCharacter;

    public List<PlayerTier> redTiers;
    public List<PlayerTier> blueTiers;

    public bool isRedWin;
    public bool isBlueWin;

    public bool isDraw;
    public float battleTime;

    public BattleStatistics statistics;

    public BattleResult(List<int> redCharacter, List<int> blueCharacter, List<PlayerTier> redTiers, List<PlayerTier> blueTiers, bool isRedWin, bool isBlueWin, BattleStatistics statistics)
    {
        this.redCharacter = redCharacter;
        this.blueCharacter = blueCharacter;

        this.redTiers = redTiers;
        this.blueTiers = blueTiers;

        this.isRedWin = isRedWin;
        this.isBlueWin = isBlueWin;

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
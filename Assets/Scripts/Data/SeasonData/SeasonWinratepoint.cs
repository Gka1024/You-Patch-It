[System.Serializable]
public class SeasonWinRatePoint
{
    public int season;
    public float winRate;

    public SeasonWinRatePoint(int season, float winRate)
    {
        this.season = season;
        this.winRate = winRate;
    }
}
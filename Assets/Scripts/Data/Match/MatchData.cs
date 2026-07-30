public class MatchData
{
    public RuntimePlayer redPlayer;
    public RuntimePlayer bluePlayer;

    public RuntimeCharacter redCharacter;
    public RuntimeCharacter blueCharacter;

    public MatchData(RuntimePlayer red, RuntimePlayer blue, RuntimeCharacter redCharacter, RuntimeCharacter blueCharacter)
    {
        this.redPlayer = red;
        this.bluePlayer = blue;

        this.redCharacter = redCharacter;
        this.blueCharacter = blueCharacter;
    }
}

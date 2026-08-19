using System.Collections.Generic;

public class MatchData
{
    public List<RuntimePlayer> redPlayers;
    public List<RuntimePlayer> bluePlayers;

    public List<RuntimeCharacter> redCharacters;
    public List<RuntimeCharacter> blueCharacters;

    public MatchData(List<RuntimePlayer> red, List<RuntimePlayer> blue, List<RuntimeCharacter> redCharacter, List<RuntimeCharacter> blueCharacter)
    {
        this.redPlayers = red;
        this.bluePlayers = blue;

        this.redCharacters = redCharacter;
        this.blueCharacters = blueCharacter;
    }
}

using System;

public class BattleAIState
{
    public float preferredDistance { get; private set; }
    public float reactionTime { get; private set; }

    private readonly BattleAI originAI;
    private readonly PlayerProfile playerProfile;

    public BattleAIState(BattleAI originAI, PlayerProfile profile, Random random)
    {
        this.originAI = originAI;
        this.playerProfile = profile;
        Initialize(random);
    }

    private void Initialize(Random random)
    {
        preferredDistance = GetRandomValue(originAI.prefferedDistance, playerProfile.consistency, random);
        reactionTime = GetReactionTime(playerProfile.reactionTime, playerProfile.consistency, random);
    }

    public BattleAction DecideAction(BattleCharacter self, BattleCharacter enemy)
    {
        return originAI.DecideAction(self, enemy, this);
    }

    private float GetRandomValue(float baseValue, float consistency, Random random)
    {
        float variationPercent = (100f - consistency) / 100f;
        float variation = baseValue * variationPercent * 0.2f;

        return baseValue + RandomRange(random, -variation * 2, variation * 2);
    }

    private float GetReactionTime(float reaction, float consistency, Random random)
    {
        float baseReactionTime = (100f - reaction) / 100f * 0.5f;
        float variation = (100f - consistency) / 100f * 0.05f;

        return Math.Max(0f, baseReactionTime + RandomRange(random, -variation, variation));
    }

    private float RandomRange(Random random, float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }

}
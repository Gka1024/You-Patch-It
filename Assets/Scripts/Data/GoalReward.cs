using System.Collections.Generic;

[System.Serializable]
public class GoalReward
{
    public int DevelopResource;
    public int TrustPoint;

    public GoalReward(int DR, int TP)
    {
        DevelopResource = DR;
        TrustPoint = TP;
    }
}
using UnityEngine;

public abstract class BattleAI : ScriptableObject
{
    [Header("Combat Style")]
    public float prefferedDistance;

    public abstract BattleAction DecideAction(BattleCharacter self, BattleCharacter enemy, BattleAIState state);

}
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/AI/RangedAI")]
public class RangedBattleAI : BattleAI
{
    public RangedBattleAI(float prefferedDistance = 5f)
    {
        this.prefferedDistance = prefferedDistance;
    }

    public override BattleAction DecideAction(BattleCharacter self, BattleCharacter enemy, BattleAIState state)
    {
        float distance = Mathf.Abs(self.position - enemy.position);

        if (distance > self.attackRange)
            return BattleAction.MoveTowards;

        if (distance < state.preferredDistance)
        {
            if (self.CanAttack)
                return BattleAction.Attack;

            return BattleAction.MoveAway;
        }

        if (self.CanAttack)
            return BattleAction.Attack;

        return BattleAction.None;

    }
}
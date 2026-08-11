using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/AI/SupportAI")]
public class BattleAI_Support : BattleAI
{
    public BattleAI_Support(float prefferedDistance = 5f)
    {
        this.prefferedDistance = prefferedDistance;
    }

    public override BattleAction DecideAction(BattleCharacter self, BattleCharacter enemy, BattleAIState state)
    {
        float distance = Mathf.Abs(self.position - enemy.position);

        if (distance > self.attackRange)
            return BattleAction.MoveTowards;

        if (distance < state.PreferredDistance)
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
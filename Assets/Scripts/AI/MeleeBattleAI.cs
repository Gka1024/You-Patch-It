using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/AI/MeleeAI")]
public class MeleeBattleAI : BattleAI
{
    public override BattleAction DecideAction(BattleCharacter self, BattleCharacter enemy, BattleAIState state)
    {
        float distance = Mathf.Abs(self.position - enemy.position);

        if (distance > self.attackRange)
        {
            return BattleAction.MoveTowards;
        }

        return BattleAction.Attack;
    }
}
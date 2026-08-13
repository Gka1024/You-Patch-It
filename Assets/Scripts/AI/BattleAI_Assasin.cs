using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/AI/AssasinAI")]
public class BattleAI_Assasin : BattleAI
{
    public override BattleAction DecideAction(BattleCharacter self, BattleCharacter enemy, BattleAIState state)
    {
        float distance = Mathf.Abs(self.position - enemy.position);

        if (distance > self.GetStat(CharacterStatType.AttackRange))
        {
            if (self.CanUseSkill && self.skill.ignoreDistance)
            {
                return BattleAction.UseSkill;
            }

            return BattleAction.MoveTowards;
        }

        if (self.CanUseSkill)
        {
            return BattleAction.UseSkill;
        }

        if (self.CanAttack)
        {
            return BattleAction.Attack;
        }

        return BattleAction.None;
    }
}
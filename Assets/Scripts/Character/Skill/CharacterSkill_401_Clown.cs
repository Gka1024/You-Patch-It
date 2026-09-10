using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Assasin")]
public class CharacterSkill_Clown : CharacterSkill
{
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float stunDuration = 1f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random);

        if (targets.Count == 0)
            return;

        BattleCharacter target = targets[0];

        float moveDistance = self.GetStat(CharacterStatType.MoveSpeed) * coefficient;
        BattleActionExecutor.MoveTowards(self, target, moveDistance);

        float damage = self.GetStat(CharacterStatType.Attack) * coefficient * damageMultiplier;
        BattleActionExecutor.DealDamage(self, target, damage);

        BattleActionExecutor.AddStun(target, stunDuration);
    }
}
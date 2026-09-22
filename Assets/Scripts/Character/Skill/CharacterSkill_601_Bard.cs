using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Bard")]
public class CharacterSkill_Bard : CharacterSkill
{
    private const float BuffDuration = 3f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, enemies, allies, random);

        foreach (BattleCharacter target in targets)
        {
            target.AddModifier(new BattleStatModifier(CharacterStatType.Defence, BattleStatModifierType.Percent, coefficient, coefficient * BuffDuration));
            BattleActionExecutor.AddShield(target, self.GetStat(CharacterStatType.Health) * coefficient, coefficient * BuffDuration);
        }

        //Debug.Log(targets[0].runtimeCharacter.OriginCharacter.characterName);
    }
}
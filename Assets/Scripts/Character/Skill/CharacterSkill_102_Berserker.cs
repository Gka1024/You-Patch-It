using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Berserker")]
public class CharacterSkill_Berserker : CharacterSkill
{
    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        throw new System.NotImplementedException();
    }
}
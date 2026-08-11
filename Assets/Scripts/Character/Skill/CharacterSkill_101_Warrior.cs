using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Warrior")]
public class CharacterSkill_Warrior : CharacterSkill
{
    public override void Execute(BattleCharacter self, BattleCharacter enemy, float coefficient)
    {
        Debug.Log("이거 기사 스킬임");
    }
}
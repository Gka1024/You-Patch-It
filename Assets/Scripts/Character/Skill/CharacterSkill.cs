using UnityEngine;

public abstract class CharacterSkill : ScriptableObject
{
    public int skillOwner;

    public string skillName;
    [TextArea] public string skillDescription;

    public bool ignoreDistance = false;

    public abstract void Execute(BattleCharacter self, BattleCharacter enemy, float coefficient);


}
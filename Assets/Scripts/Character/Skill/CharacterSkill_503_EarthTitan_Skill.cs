using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/EarthTitan")]
public class CharacterSkill_503_EarthTitan_Skill : CharacterSkill
{
    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        skillEffectInfos.Add(new SkillEffectInfo("대지거인이 스킬 사용시 얻는 효과",0,"\n이걸 왜 읽고 계신가요?"));
    }
}

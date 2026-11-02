using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/TarotReader")]
public class CharacterSkill_603_TarotReader_Skill : CharacterSkill
{
    [Header("Debuff Settings")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private float attackReduction = 0.25f;
    [SerializeField] private float attackSpeedReduction = 0.25f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        foreach (BattleCharacter enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            // 공격력 25% 감소
            enemy.AddModifier(new BattleStatModifier(
                CharacterStatType.Attack,
                BattleStatModifierType.Percent,
                -attackReduction,
                duration
            ));

            // 공격 속도 25% 감소
            enemy.AddModifier(new BattleStatModifier(
                CharacterStatType.AttackSpeed,
                BattleStatModifierType.Percent,
                -attackSpeedReduction,
                duration
            ));
        }
    }
}
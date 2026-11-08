using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Falcon")]
public class CharacterSkill_Falcon : CharacterSkill
{
    [Header("Hawk Summon")]
    [SerializeField] private float attackBonusCoefficient = 0.3f;

    [Header("Hawk Sacrifice")]
    [SerializeField] private float sacrificeDamageCoefficient = 2.5f;

    private float CalculateAttackBonus(float coefficient)
    {
        return attackBonusCoefficient * coefficient;
    }

    private float CalculateSacrificeDamage(float attack, float coefficient)
    {
        return attack * sacrificeDamageCoefficient * coefficient;
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        bool isHawkSummoned = self.skillStack == 1;

        if (!isHawkSummoned)
        {
            self.StartSkill(new BattleSkillInstance_Falcon(self, null, false, CalculateAttackBonus(coefficient), 0f));
            return;
        }

        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        if (targets.Count == 0)
            return;

        self.StartSkill(new BattleSkillInstance_Falcon(self, targets[0], true, 0f, sacrificeDamageCoefficient * coefficient));
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);
        float attack = character.GetStat(CharacterStatType.Attack);

        float attackBonus = CalculateAttackBonus(coefficient);
        float sacrificeDamage = CalculateSacrificeDamage(attack, coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("[매 소환] 공격력 증가", attackBonus * 100f, "%"));
        skillEffectInfos.Add(new SkillEffectInfo("[매 희생] 피해량", sacrificeDamage));
    }


    private class BattleSkillInstance_Falcon : BattleSkillInstance
    {
        private readonly BattleCharacter target;
        private readonly bool isHawkSummoned;
        private readonly float attackBonusCoefficient;
        private readonly float sacrificeDamageCoefficient;

        public BattleSkillInstance_Falcon(BattleCharacter self, BattleCharacter target, bool isHawkSummoned, float attackBonusCoefficient, float sacrificeDamageCoefficient) : base(self, null, null, 0f, null)
        {
            this.target = target;
            this.isHawkSummoned = isHawkSummoned;
            this.attackBonusCoefficient = attackBonusCoefficient;
            this.sacrificeDamageCoefficient = sacrificeDamageCoefficient;
        }

        protected override void OnStart()
        {
            if (self.IsDead)
            {
                Finish();
                return;
            }

            if (isHawkSummoned)
                SacrificeHawk();
            else
                SummonHawk();

            Finish();
        }

        private void SummonHawk()
        {
            BattleStatModifier modifier = new BattleStatModifier(
                CharacterStatType.Attack,
                BattleStatModifierType.Percent,
                attackBonusCoefficient,
                float.MaxValue
            );

            self.AddModifier(modifier);
            self.skillStack = 1;
        }

        private void SacrificeHawk()
        {
            if (target == null || target.IsDead)
                return;

            float damage = self.GetStat(CharacterStatType.Attack) * sacrificeDamageCoefficient;

            BattleActionExecutor.DealDamage(self, target, damage);

            self.RemoveModifiers(CharacterStatType.Attack, BattleStatModifierType.Percent, attackBonusCoefficient);
            self.skillStack = 0;
        }

        protected override void OnTick(float tick)
        {
            Finish();
        }
    }
}
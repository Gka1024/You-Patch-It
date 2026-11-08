using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Lancer")]
public class CharacterSkill_104_Lancer_skill : CharacterSkill
{
    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 3f;

    [Header("Pull")]
    [SerializeField] private float pullDistance = 3f;
    [SerializeField] private float damageCoefficient = 1f;

    private float CalculateDamage(float attack, float coefficient)
    {
        return attack * damageCoefficient * coefficient;
    }

    private float CalculateRangeIncrease(float attackRange, float coefficient)
    {
        return 2f + attackRange * (Mathf.Max(1f, coefficient) - 1f);
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        if (targets.Count == 0)
            return;

        BattleCharacter target = targets[0];
        bool isMeleeStance = self.skillStack == 0;

        float rangeIncreaseValue = CalculateRangeIncrease(
            self.GetStat(CharacterStatType.AttackRange),
            coefficient
        );

        BattleSkillInstance_104_Lancer skillInstance = new BattleSkillInstance_104_Lancer(
            self,
            target,
            rangeIncreaseValue,
            knockbackDistance,
            pullDistance,
            damageCoefficient * coefficient,
            isMeleeStance
        );

        if (self.StartSkill(skillInstance))
            self.skillStack = isMeleeStance ? 1 : 0;
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);
        float attack = character.GetStat(CharacterStatType.Attack);
        float attackRange = character.GetStat(CharacterStatType.AttackRange);

        float damage = CalculateDamage(attack, coefficient);
        float rangeIncreaseValue = CalculateRangeIncrease(attackRange, coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("[근접 자세] 넉백 거리", knockbackDistance));
        skillEffectInfos.Add(new SkillEffectInfo("[근접 자세] 공격 사거리 증가", rangeIncreaseValue));
        skillEffectInfos.Add(new SkillEffectInfo("[원거리 자세] 끌어당기는 거리", pullDistance));
        skillEffectInfos.Add(new SkillEffectInfo("[원거리 자세] 피해량", damage));
    }

    private class BattleSkillInstance_104_Lancer : BattleSkillInstance
    {
        private readonly BattleCharacter target;
        private readonly float rangeIncrease;
        private readonly float knockbackDistance;
        private readonly float pullDistance;
        private readonly float damageCoefficient;
        private readonly bool isMeleeStance;

        public BattleSkillInstance_104_Lancer(BattleCharacter self, BattleCharacter target, float rangeIncrease, float knockbackDistance, float pullDistance, float damageCoefficient, bool isMeleeStance) : base(self, null, null, 0f, null)
        {
            this.target = target;
            this.rangeIncrease = rangeIncrease;
            this.knockbackDistance = knockbackDistance;
            this.pullDistance = pullDistance;
            this.damageCoefficient = damageCoefficient;
            this.isMeleeStance = isMeleeStance;
        }

        protected override void OnStart()
        {
            if (self.IsDead || target == null || target.IsDead)
            {
                Finish();
                return;
            }

            if (isMeleeStance)
            {
                KnockbackTarget();
                ApplyRangeBuff();
            }
            else
            {
                PullTargetAndDealDamage();
                RemoveRangeBuff();
            }

            Finish();
        }

        private void KnockbackTarget()
        {
            float direction = Mathf.Sign(target.position - self.position);

            if (direction == 0f)
                direction = 1f;

            target.position += direction * knockbackDistance;
            target.statistics.moveDistance += knockbackDistance;
        }

        private void ApplyRangeBuff()
        {
            BattleStatModifier modifier = new BattleStatModifier(
                CharacterStatType.AttackRange,
                BattleStatModifierType.Flat,
                rangeIncrease,
                float.MaxValue
            );

            self.AddModifier(modifier);
        }

        private void PullTargetAndDealDamage()
        {
            float distanceToTarget = Mathf.Abs(target.position - self.position);
            float moveDistance = Mathf.Min(pullDistance, distanceToTarget);
            float direction = Mathf.Sign(target.position - self.position);

            if (direction == 0f)
                direction = 1f;

            // 적을 시전자 방향으로 끌어당긴다.
            target.position -= direction * moveDistance;
            target.statistics.moveDistance += moveDistance;

            // 기본 공격력에 계수를 곱해 피해를 계산한다.
            float damage = self.runtimeCharacter.GetStat(CharacterStatType.Attack) * damageCoefficient;

            BattleActionExecutor.DealDamage(self, target, damage);
        }

        private void RemoveRangeBuff()
        {
            self.RemoveModifiers(CharacterStatType.AttackRange, BattleStatModifierType.Flat, rangeIncrease);
        }

        protected override void OnTick(float tick)
        {
            Finish();
        }
    }
}
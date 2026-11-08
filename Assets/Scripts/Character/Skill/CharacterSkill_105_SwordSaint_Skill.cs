using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/SwordSaint")]
public class CharacterSkill_105_SwordSaint_Skill : CharacterSkill
{
    private const int attackIntervalTicks = 10;
    private const float tick = 0.05f;

    public override void OnAttackHit(BattleCharacter self)
    {
        self.skillStack++;
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        if (self.skillStack <= 0)
            return;

        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        if (targets.Count == 0)
            return;

        int attackCount = self.skillStack;
        self.skillStack = 0;

        self.StartSkill(new BattleSkillInstance_105_SwordSaint(self, targets[0], attackCount, coefficient));
    }

    private float CalculateDamage(float attack, float coefficient)
    {
        return attack * coefficient;
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attack = character.GetStat(CharacterStatType.Attack);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        float damage = CalculateDamage(attack, coefficient);
        float attackInterval = attackIntervalTicks * tick;

        skillEffectInfos.Add(new SkillEffectInfo("일반 공격 적중 시 스택 획득", 1f, "회"));
        skillEffectInfos.Add(new SkillEffectInfo("스택당 공격 횟수", 1f, "회"));
        skillEffectInfos.Add(new SkillEffectInfo("공격 1회당 피해량", damage));
        skillEffectInfos.Add(new SkillEffectInfo("공격 간격", attackInterval, "초"));
    }


    private class BattleSkillInstance_105_SwordSaint : BattleSkillInstance
    {
        private const int ATTACK_INTERVAL_TICKS = 10;
        private const float TICK = 0.05f;

        private readonly BattleCharacter target;
        private readonly int totalAttackCount;

        private int currentAttackCount;
        private int remainingTicks;

        public BattleSkillInstance_105_SwordSaint(BattleCharacter self, BattleCharacter target, int attackCount, float coefficient) : base(self, null, null, coefficient, null)
        {
            this.target = target;
            totalAttackCount = attackCount;
        }

        protected override void OnStart()
        {
            Attack();

            remainingTicks = ATTACK_INTERVAL_TICKS;
        }

        protected override void OnTick(float tick)
        {
            if (target == null || target.IsDead)
            {
                Finish();
                return;
            }

            remainingTicks--;

            if (remainingTicks > 0)
                return;

            Attack();

            remainingTicks = ATTACK_INTERVAL_TICKS;
        }

        private void Attack()
        {
            if (target == null || target.IsDead || self.IsDead)
            {
                Finish();
                return;
            }

            float damage = self.GetStat(CharacterStatType.Attack) * coefficient;
            BattleActionExecutor.DealDamage(self, target, damage);

            currentAttackCount++;

            if (currentAttackCount >= totalAttackCount)
                Finish();
        }
    }
}
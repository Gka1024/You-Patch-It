using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Chakramer")]
public class CharacterSkill_204_Chakramer_Skill : CharacterSkill
{
    [Header("Chakram")]
    [SerializeField] private int tickInterval = 5;
    [SerializeField] private int maxBounceCount = 5;
    [SerializeField] private float damageCoefficient = 0.5f;

    private float CalculateDamage(float attack, float coefficient)
    {
        return attack * damageCoefficient * coefficient;
    }

    private float CalculateAttackInterval()
    {
        return tickInterval * 0.05f;
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        if (targets.Count == 0)
            return;

        self.StartSkill(new BattleSkillInstance_204_Chakramer(self, enemies, tickInterval, maxBounceCount, damageCoefficient * coefficient, random));
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attack = character.GetStat(CharacterStatType.Attack);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        float damage = CalculateDamage(attack, coefficient);
        float attackInterval = CalculateAttackInterval();
        float totalDamage = damage * maxBounceCount;

        skillEffectInfos.Add(new SkillEffectInfo("1회 피해량", damage));
        skillEffectInfos.Add(new SkillEffectInfo("최대 튕김 횟수", maxBounceCount, "회"));
        skillEffectInfos.Add(new SkillEffectInfo("튕김 간격", attackInterval, "초"));
        skillEffectInfos.Add(new SkillEffectInfo("최대 총 피해량", totalDamage));

    }

    private class BattleSkillInstance_204_Chakramer : BattleSkillInstance
    {
        public override bool BlocksActions => false;

        private readonly int tickInterval;
        private readonly int maxBounceCount;
        private readonly float damageCoefficient;

        private int tickCounter;
        private int bounceCount;

        private BattleCharacter lastTarget;

        public BattleSkillInstance_204_Chakramer(BattleCharacter self, List<BattleCharacter> enemies, int tickInterval, int maxBounceCount, float damageCoefficient, System.Random random) : base(self, null, enemies, 0f, random)
        {
            this.tickInterval = Mathf.Max(1, tickInterval);
            this.maxBounceCount = Mathf.Max(1, maxBounceCount);
            this.damageCoefficient = damageCoefficient;
        }

        protected override void OnStart()
        {
            if (self.IsDead || enemies == null || enemies.Count == 0)
            {
                Finish();
                return;
            }

            tickCounter = 0;
            bounceCount = 0;
            lastTarget = null;
        }

        protected override void OnTick(float tick)
        {
            if (self.IsDead || bounceCount >= maxBounceCount)
            {
                Finish();
                return;
            }

            tickCounter++;

            if (tickCounter < tickInterval)
                return;

            tickCounter = 0;

            BattleCharacter target = FindNextTarget();

            if (target == null)
            {
                Finish();
                return;
            }

            float damage = self.GetStat(CharacterStatType.Attack) * damageCoefficient;
            BattleActionExecutor.DealDamage(self, target, damage);

            lastTarget = target;
            bounceCount++;

            if (bounceCount >= maxBounceCount)
                Finish();
        }

        private BattleCharacter FindNextTarget()
        {
            BattleCharacter nearestTarget = null;
            float nearestDistance = float.MaxValue;

            float originPosition = lastTarget != null && !lastTarget.IsDead ? lastTarget.position : self.position;

            for (int i = 0; i < enemies.Count; i++)
            {
                BattleCharacter enemy = enemies[i];

                if (enemy == null || enemy.IsDead)
                    continue;

                // 이전 대상이 살아 있고 다른 적도 있다면 같은 대상에게 연속으로 튕기지 않음
                if (lastTarget != null && enemy == lastTarget)
                    continue;

                float distance = Mathf.Abs(enemy.position - originPosition);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = enemy;
                }
            }

            // 다른 대상이 없다면 마지막 대상에게 다시 튕길 수 있음
            if (nearestTarget == null && lastTarget != null && !lastTarget.IsDead)
                return lastTarget;

            return nearestTarget;
        }

        protected override void OnCancel() { }

        protected override void OnFinish() { }
    }
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Alchemist")]
public class CharacterSkill_304_Alchemist_Skill : CharacterSkill
{
    [Header("Potion Settings")]
    [SerializeField] private float areaRadius = 2.5f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float tickInterval = 1f;

    [Header("Enemy Effects")]
    [SerializeField] private float damageRatio = 0.2f;
    [SerializeField] private float slowRatio = 0.25f;

    [Header("Ally Effects")]
    [SerializeField] private float healRatio = 0.15f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        // CreateInstance에서 생성한 BattleSkillInstance를 사용한다.
    }

    public override BattleSkillInstance CreateInstance(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        if (targets.Count == 0)
            return null;

        BattleCharacter target = targets[0];

        return new AlchemistSkillInstance(
            self,
            allies,
            enemies,
            coefficient,
            random,
            target.position,
            areaRadius,
            duration,
            tickInterval,
            damageRatio,
            healRatio,
            slowRatio
        );
    }

    private class AlchemistSkillInstance : BattleSkillInstance
    {
        private readonly float impactPosition;
        private readonly float areaRadius;
        private readonly float duration;
        private readonly float tickInterval;
        private readonly float damageRatio;
        private readonly float healRatio;
        private readonly float slowRatio;

        private readonly List<BattleCharacter> affectedAllies = new();
        private readonly List<BattleCharacter> affectedEnemies = new();
        private readonly List<(BattleCharacter target, BattleStatModifier modifier)> slowModifiers = new();

        private float elapsedTime;
        private float tickTimer;

        public override bool BlocksActions => true;

        public AlchemistSkillInstance(
            BattleCharacter self,
            List<BattleCharacter> allies,
            List<BattleCharacter> enemies,
            float coefficient,
            System.Random random,
            float impactPosition,
            float areaRadius,
            float duration,
            float tickInterval,
            float damageRatio,
            float healRatio,
            float slowRatio)
            : base(self, allies, enemies, coefficient, random)
        {
            this.impactPosition = impactPosition;
            this.areaRadius = areaRadius;
            this.duration = duration;
            this.tickInterval = Mathf.Max(0.01f, tickInterval);
            this.damageRatio = damageRatio;
            this.healRatio = healRatio;
            this.slowRatio = slowRatio;
        }

        protected override void OnStart()
        {
            ApplyAreaEffects();
            ApplyInitialTick();
        }

        private void ApplyAreaEffects()
        {
            foreach (BattleCharacter ally in allies)
            {
                if (ally == null || ally.IsDead)
                    continue;

                if (Mathf.Abs(ally.position - impactPosition) > areaRadius)
                    continue;

                if (!affectedAllies.Contains(ally))
                    affectedAllies.Add(ally);
            }

            foreach (BattleCharacter enemy in enemies)
            {
                if (enemy == null || enemy.IsDead)
                    continue;

                if (Mathf.Abs(enemy.position - impactPosition) > areaRadius)
                    continue;

                if (!affectedEnemies.Contains(enemy))
                    affectedEnemies.Add(enemy);

                BattleStatModifier modifier = new BattleStatModifier(
                    CharacterStatType.MoveSpeed,
                    BattleStatModifierType.Percent,
                    -slowRatio,
                    duration,
                    this
                );

                enemy.AddModifier(modifier);
                slowModifiers.Add((enemy, modifier));
            }
        }

        private void ApplyInitialTick()
        {
            ApplyDamageAndHealing();
        }

        protected override void OnTick(float tick)
        {
            elapsedTime += tick;
            tickTimer += tick;

            while (tickTimer >= tickInterval && elapsedTime < duration)
            {
                tickTimer -= tickInterval;
                ApplyDamageAndHealing();
            }

            if (elapsedTime >= duration)
                Finish();
        }

        private void ApplyDamageAndHealing()
        {
            float damage = self.GetStat(CharacterStatType.Attack) * damageRatio * coefficient;

            foreach (BattleCharacter enemy in affectedEnemies)
            {
                if (enemy == null || enemy.IsDead)
                    continue;

                BattleActionExecutor.DealDamage(self, enemy, damage);
            }

            foreach (BattleCharacter ally in affectedAllies)
            {
                if (ally == null || ally.IsDead)
                    continue;

                float healing = ally.GetStat(CharacterStatType.Health) * healRatio * coefficient;

                ally.currentHealth = Mathf.Min(
                    ally.GetStat(CharacterStatType.Health),
                    ally.currentHealth + healing
                );
            }
        }

        protected override void OnFinish()
        {
            RemoveSlowEffects();
        }

        protected override void OnCancel()
        {
            RemoveSlowEffects();
        }

        private void RemoveSlowEffects()
        {
            foreach ((BattleCharacter target, BattleStatModifier modifier) in slowModifiers)
            {
                if (target != null)
                    target.RemoveModifier(modifier);
            }

            slowModifiers.Clear();
            affectedAllies.Clear();
            affectedEnemies.Clear();
        }
    }
}
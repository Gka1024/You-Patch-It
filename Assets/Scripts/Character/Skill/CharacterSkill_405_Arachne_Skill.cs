using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Arachne")]
public class CharacterSkill_402_Arachne_Skill : CharacterSkill
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float stopDistance = 0.5f;

    [Header("Stun Settings")]
    [SerializeField] private float stunDuration = 1.5f;

    [Header("Attack Settings")]
    [SerializeField] private float damageRatio = 2f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        // BattleSkillInstance를 통해 실행한다.
    }

    public override BattleSkillInstance CreateInstance(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        if (targets.Count == 0)
            return null;

        return new ArachneSkillInstance(
            self,
            allies,
            enemies,
            coefficient,
            random,
            targets[0],
            dashSpeed,
            stopDistance,
            stunDuration,
            damageRatio
        );
    }

    private class ArachneSkillInstance : BattleSkillInstance
    {
        private readonly BattleCharacter target;
        private readonly float dashSpeed;
        private readonly float stopDistance;
        private readonly float stunDuration;
        private readonly float damageRatio;

        private bool hasReachedTarget;
        private bool hasAttacked;

        public override bool BlocksActions => true;

        public ArachneSkillInstance(
            BattleCharacter self,
            List<BattleCharacter> allies,
            List<BattleCharacter> enemies,
            float coefficient,
            System.Random random,
            BattleCharacter target,
            float dashSpeed,
            float stopDistance,
            float stunDuration,
            float damageRatio)
            : base(self, allies, enemies, coefficient, random)
        {
            this.target = target;
            this.dashSpeed = dashSpeed;
            this.stopDistance = stopDistance;
            this.stunDuration = stunDuration;
            this.damageRatio = damageRatio;
        }

        private void MoveTowardsTarget(float tick)
        {
            float distance = Mathf.Abs(target.position - self.position);

            if (distance <= stopDistance)
            {
                hasReachedTarget = true;
                return;
            }

            float direction = Mathf.Sign(target.position - self.position);
            float moveDistance = dashSpeed * tick;

            if (moveDistance >= distance - stopDistance)
            {
                self.position = target.position - direction * stopDistance;
                hasReachedTarget = true;
                return;
            }

            self.position += direction * moveDistance;
        }

        protected override void OnStart()
        {
            if (target == null || target.IsDead)
            {
                Finish();
                return;
            }
        }

        protected override void OnTick(float tick)
        {
            if (target == null || target.IsDead)
            {
                Finish();
                return;
            }

            if (!hasReachedTarget)
            {
                MoveTowardsTarget(tick);
                return;
            }

            if (!hasAttacked)
            {
                ApplyStun();
                ApplyDamage();
                hasAttacked = true;
            }

            Finish();
        }

        private void ApplyStun()
        {
            BattleActionExecutor.AddStun(target, stunDuration);
        }

        private void ApplyDamage()
        {
            float damage = self.GetStat(CharacterStatType.Attack) * damageRatio * coefficient;
            BattleActionExecutor.DealDamage(self, target, damage);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Reaper")]
public class CharacterSkill_403_Reaper_Skill : CharacterSkill
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float stopDistance = 0.5f;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 1.5f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float damageRatio = 3f;

    private float CalculateDamage(float attack, float coefficient)
    {
        return attack * damageRatio * coefficient;
    }
    
    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        // CreateInstance()에서 생성한 BattleSkillInstance를 사용한다.
    }

    public override BattleSkillInstance CreateInstance(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        if (targets.Count == 0)
            return null;

        return new ReaperSkillInstance(
            self,
            allies,
            enemies,
            coefficient,
            random,
            targets[0],
            dashSpeed,
            stopDistance,
            invincibilityDuration,
            attackRange,
            damageRatio
        );
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attack = character.GetStat(CharacterStatType.Attack);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        float damage = CalculateDamage(attack, coefficient);

        skillEffectInfos.Add(new SkillEffectInfo("돌진 속도", dashSpeed));
        skillEffectInfos.Add(new SkillEffectInfo("돌진 정지 거리", stopDistance));
        skillEffectInfos.Add(new SkillEffectInfo("무적 지속 시간", invincibilityDuration, "초"));
        skillEffectInfos.Add(new SkillEffectInfo("공격 범위", attackRange));
        skillEffectInfos.Add(new SkillEffectInfo("광역 피해량", damage));
    }

}

public class ReaperSkillInstance : BattleSkillInstance
{
    private readonly BattleCharacter target;
    private readonly float dashSpeed;
    private readonly float stopDistance;
    private readonly float invincibilityDuration;
    private readonly float attackRange;
    private readonly float damageRatio;

    private float elapsedTime;
    private bool hasReachedTarget;
    private bool hasAttacked;

    public override bool BlocksActions => true;

    public ReaperSkillInstance(
        BattleCharacter self,
        List<BattleCharacter> allies,
        List<BattleCharacter> enemies,
        float coefficient,
        System.Random random,
        BattleCharacter target,
        float dashSpeed,
        float stopDistance,
        float invincibilityDuration,
        float attackRange,
        float damageRatio)
        : base(self, allies, enemies, coefficient, random)
    {
        this.target = target;
        this.dashSpeed = dashSpeed;
        this.stopDistance = stopDistance;
        this.invincibilityDuration = invincibilityDuration;
        this.attackRange = attackRange;
        this.damageRatio = damageRatio;
    }

    protected override void OnStart()
    {
        if (target == null || target.IsDead)
        {
            Finish();
            return;
        }

        self.isInvulnerable = true;
    }

    protected override void OnTick(float tick)
    {
        if (target == null || target.IsDead)
        {
            EndSkill();
            return;
        }

        if (!hasReachedTarget)
        {
            MoveTowardsTarget(tick);
            return;
        }

        elapsedTime += tick;

        if (elapsedTime >= invincibilityDuration)
        {
            if (!hasAttacked)
            {
                hasAttacked = true;
                DealAreaDamage();
            }

            EndSkill();
        }
    }

    private void MoveTowardsTarget(float tick)
    {
        float distance = Mathf.Abs(target.position - self.position);

        if (distance <= stopDistance)
        {
            hasReachedTarget = true;
            elapsedTime = 0f;
            return;
        }

        float direction = Mathf.Sign(target.position - self.position);
        float moveDistance = dashSpeed * tick;

        if (moveDistance >= distance - stopDistance)
        {
            self.position = target.position - direction * stopDistance;
            hasReachedTarget = true;
            elapsedTime = 0f;
            return;
        }

        self.position += direction * moveDistance;
    }

    private void DealAreaDamage()
    {
        float damage = self.GetStat(CharacterStatType.Attack) * damageRatio * coefficient;

        foreach (BattleCharacter enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            float distance = Mathf.Abs(enemy.position - self.position);

            if (distance > attackRange)
                continue;

            BattleActionExecutor.DealDamage(self, enemy, damage);
        }
    }

    private void EndSkill()
    {
        self.isInvulnerable = false;
        Finish();
    }

    protected override void OnCancel()
    {
        self.isInvulnerable = false;
    }

    protected override void OnFinish()
    {
        self.isInvulnerable = false;
    }
}
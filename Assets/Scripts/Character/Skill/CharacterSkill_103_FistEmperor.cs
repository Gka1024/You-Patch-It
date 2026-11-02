using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/FistEmperor")]
public class CharacterSkill_103_FistEmperor : CharacterSkill
{
    [SerializeField] private float baseHealthDamageCoefficient = 0.1f;
    [SerializeField] private float knockbackDistance = 3f;
    [SerializeField] private float damageRadius = 2f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        if (targets.Count == 0)
            return;

        BattleCharacter target = targets[0];

        float additionalSkillCoefficient = Mathf.LerpUnclamped(0, 0.1f, coefficient);

        self.StartSkill(new BattleSkillInstance_103_FistEmperor(self, target, enemies, baseHealthDamageCoefficient + additionalSkillCoefficient, knockbackDistance, damageRadius));
    }
}

public class BattleSkillInstance_103_FistEmperor : BattleSkillInstance
{
    private readonly BattleCharacter target;
    private readonly List<BattleCharacter> enemies;
    private readonly float healthDamageCoefficient;
    private readonly float knockbackDistance;
    private readonly float damageRadius;

    public BattleSkillInstance_103_FistEmperor(BattleCharacter self, BattleCharacter target, List<BattleCharacter> enemies, float healthDamageCoefficient, float knockbackDistance, float damageRadius) : base(self, null, null, 0f, null)
    {
        this.target = target;
        this.enemies = enemies;
        this.healthDamageCoefficient = healthDamageCoefficient;
        this.knockbackDistance = knockbackDistance;
        this.damageRadius = damageRadius;
    }

    protected override void OnStart()
    {
        if (self.IsDead || target == null || target.IsDead)
        {
            Finish();
            return;
        }

        // 밀쳐내기 직전의 체력을 기준으로 피해량 계산
        float damage = target.currentHealth * healthDamageCoefficient;

        // 적을 시전자의 반대 방향으로 즉시 밀쳐낸다.
        float direction = Mathf.Sign(target.position - self.position);

        if (direction == 0f)
            direction = 1f;

        target.position += direction * knockbackDistance;
        target.statistics.moveDistance += knockbackDistance;

        // 밀쳐낸 적의 최종 위치를 기준으로 주변 적에게 피해를 준다.
        for (int i = 0; i < enemies.Count; i++)
        {
            BattleCharacter enemy = enemies[i];

            if (enemy == null || enemy.IsDead)
                continue;

            float distance = Mathf.Abs(enemy.position - target.position);

            if (distance > damageRadius)
                continue;

            BattleActionExecutor.DealDamage(self, enemy, damage);
        }

        Finish();
    }

    protected override void OnTick(float tick)
    {
        Finish();
    }
}
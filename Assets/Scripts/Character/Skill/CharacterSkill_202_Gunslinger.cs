using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Gunslinger")]
public class CharacterSkill_Gunslinger : CharacterSkill
{
    [Header("Damage Over Time")]
    [SerializeField] private float damageRadius = 3f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float damageInterval = 0.5f;

    private float CalculateDamage(float attack, float coefficient)
    {
        return attack * coefficient;
    }

    private int CalculateDamageCount()
    {
        return Mathf.CeilToInt(duration / damageInterval) + 1;
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        if (self.isUsingSkills)
            return;

        self.StartSkill(new BattleSkillInstance_Gunslinger(self, enemies, coefficient, damageRadius, duration, damageInterval));
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attack = character.GetStat(CharacterStatType.Attack);
        float coefficient = character.GetStat(CharacterStatType.SkillCoefficient);

        float damage = CalculateDamage(attack, coefficient);
        int damageCount = CalculateDamageCount();
        float totalDamage = damage * damageCount;

        skillEffectInfos.Add(new SkillEffectInfo("범위", damageRadius));
        skillEffectInfos.Add(new SkillEffectInfo("지속 시간", duration, "초"));
        skillEffectInfos.Add(new SkillEffectInfo("피해 간격", damageInterval, "초"));
        skillEffectInfos.Add(new SkillEffectInfo("1회 피해량", damage));
        skillEffectInfos.Add(new SkillEffectInfo("예상 피해 횟수", damageCount, "회"));
        skillEffectInfos.Add(new SkillEffectInfo("예상 총 피해량", totalDamage));
    }


    private class BattleSkillInstance_Gunslinger : BattleSkillInstance
    {
        private readonly float damageRadius;
        private readonly float duration;
        private readonly float damageInterval;

        private float elapsedTime;
        private float damageTimer;

        public BattleSkillInstance_Gunslinger(BattleCharacter self, List<BattleCharacter> enemies, float coefficient, float damageRadius, float duration, float damageInterval) : base(self, null, enemies, coefficient, null)
        {
            this.damageRadius = damageRadius;
            this.duration = duration;
            this.damageInterval = damageInterval;
        }

        protected override void OnStart()
        {
            if (self.IsDead)
            {
                Finish();
                return;
            }

            // 스킬 사용 즉시 첫 피해를 준다.
            DealAreaDamage();
            damageTimer = damageInterval;
        }

        protected override void OnTick(float tick)
        {
            if (self.IsDead)
            {
                Finish();
                return;
            }

            elapsedTime += tick;
            damageTimer -= tick;

            if (damageTimer <= 0f)
            {
                DealAreaDamage();
                damageTimer += damageInterval;
            }

            if (elapsedTime >= duration)
                Finish();
        }

        private void DealAreaDamage()
        {
            if (enemies == null)
                return;

            float damage = self.GetStat(CharacterStatType.Attack) * coefficient;

            for (int i = 0; i < enemies.Count; i++)
            {
                BattleCharacter enemy = enemies[i];

                if (enemy == null || enemy.IsDead)
                    continue;

                float distance = Mathf.Abs(enemy.position - self.position);

                if (distance > damageRadius)
                    continue;

                BattleActionExecutor.DealDamage(self, enemy, damage);
            }
        }
    }
}
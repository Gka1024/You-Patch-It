using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skill/Painter")]
public class PainterSkill : CharacterSkill
{
    private const int MaxStack = 5;

    private const float SlowAmount = 0.2f;
    private const float DefenceReduction = 0.2f;
    private const float DamageOverTimeRatio = 0.1f;
    private const float DamageOverTimeInterval = 1f;
    private const float ManaBurnRatio = 0.1f;

    private float CalculateDamage(float attack)
    {
        return attack * DamageOverTimeRatio;
    }

    private float CalculateManaBurn(float maxMana)
    {
        return maxMana * ManaBurnRatio;
    }

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        if (targets.Count == 0)
            return;

        PainterSkillInstance instance = self.activeSkill as PainterSkillInstance;

        if (instance == null)
        {
            instance = new PainterSkillInstance(self, allies, enemies, coefficient, random);

            if (!self.StartSkill(instance))
                return;
        }

        foreach (BattleCharacter target in targets)
        {
            if (target == null || target.IsDead)
                continue;

            instance.ApplyStack(target);
        }
    }

    public override void RefreshInfo(RuntimeCharacter character)
    {
        skillEffectInfos.Clear();

        float attack = character.GetStat(CharacterStatType.Attack);
        float maxMana = character.GetStat(CharacterStatType.MaxMana);

        float damage = CalculateDamage(attack);
        float manaBurn = CalculateManaBurn(maxMana);

        skillEffectInfos.Add(new SkillEffectInfo("최대 중첩", MaxStack, "회"));
        skillEffectInfos.Add(new SkillEffectInfo("[1중첩] 이동 속도 감소", SlowAmount * 100f, "%"));
        skillEffectInfos.Add(new SkillEffectInfo("[2중첩] 방어력 감소", DefenceReduction * 100f, "%"));
        skillEffectInfos.Add(new SkillEffectInfo("[3중첩] 지속 피해량", damage));
        skillEffectInfos.Add(new SkillEffectInfo("[3중첩] 피해 간격", DamageOverTimeInterval, "초"));
        skillEffectInfos.Add(new SkillEffectInfo("[4중첩] 마나 소모량", manaBurn));
        skillEffectInfos.Add(new SkillEffectInfo("[5중첩] 처형", 100, "% 이하"));
    }


    private class PainterSkillInstance : BattleSkillInstance
    {
        private const int MaxStack = 5;

        private const float SlowAmount = 0.2f;
        private const float DefenceReduction = 0.2f;
        private const float DamageOverTimeRatio = 0.1f;
        private const float DamageOverTimeInterval = 1f;
        private const float ManaBurnRatio = 0.1f;

        private readonly Dictionary<BattleCharacter, int> targetStacks = new();
        private readonly Dictionary<BattleCharacter, float> damageOverTimeTimers = new();

        public PainterSkillInstance(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random) : base(self, allies, enemies, coefficient, random)
        {
        }

        public override bool BlocksActions => false;

        public void ApplyStack(BattleCharacter target)
        {
            if (target == null || target.IsDead)
                return;

            if (!targetStacks.TryGetValue(target, out int stack))
                stack = 0;

            if (stack >= MaxStack)
                return;

            stack++;

            targetStacks[target] = stack;

            switch (stack)
            {
                case 1:
                    ApplySlow(target);
                    break;

                case 2:
                    ApplyDefenceReduction(target);
                    break;

                case 3:
                    ApplyDamageOverTime(target);
                    break;

                case 4:
                    ApplyManaBurn(target);
                    break;

                case 5:
                    Execute(target);
                    break;
            }
        }

        private void ApplySlow(BattleCharacter target)
        {
            target.AddModifier(new BattleStatModifier(CharacterStatType.MoveSpeed, BattleStatModifierType.Percent, -SlowAmount, float.PositiveInfinity, this));
        }

        private void ApplyDefenceReduction(BattleCharacter target)
        {
            target.AddModifier(new BattleStatModifier(CharacterStatType.Defence, BattleStatModifierType.Percent, -DefenceReduction, float.PositiveInfinity, this));
        }

        private void ApplyDamageOverTime(BattleCharacter target)
        {
            damageOverTimeTimers[target] = 0f;
        }

        private void ApplyManaBurn(BattleCharacter target)
        {
            float manaBurn = target.GetStat(CharacterStatType.MaxMana) * ManaBurnRatio;

            target.currentMana = Mathf.Max(0f, target.currentMana - manaBurn);
        }

        private void Execute(BattleCharacter target)
        {
            BattleActionExecutor.ExecuteTarget(self, target);
        }

        protected override void OnTick(float tick)
        {
            TickDamageOverTime(tick);
        }

        private void TickDamageOverTime(float tick)
        {
            List<BattleCharacter> targets = new(damageOverTimeTimers.Keys);

            foreach (BattleCharacter target in targets)
            {
                if (target == null || target.IsDead)
                    continue;

                float timer = damageOverTimeTimers[target] + tick;

                while (timer >= DamageOverTimeInterval)
                {
                    timer -= DamageOverTimeInterval;

                    float damage = self.GetStat(CharacterStatType.Attack) * DamageOverTimeRatio;

                    BattleActionExecutor.DealDamage(self, target, damage);

                    if (target.IsDead)
                        break;
                }

                damageOverTimeTimers[target] = timer;
            }
        }

        protected override void OnCancel()
        {
            foreach (BattleCharacter target in targetStacks.Keys)
            {
                if (target == null)
                    continue;

                target.RemoveModifiersBySource(this);
            }

            targetStacks.Clear();
            damageOverTimeTimers.Clear();
        }
    }
}
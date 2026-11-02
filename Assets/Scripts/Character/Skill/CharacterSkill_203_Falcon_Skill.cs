using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/Skill/Hunter")]
public class CharacterSkill_Hunter : CharacterSkill
{
    [Header("Hawk Summon")]
    [SerializeField] private float attackBonusCoefficient = 0.3f;

    [Header("Hawk Sacrifice")]
    [SerializeField] private float sacrificeDamageCoefficient = 2.5f;

    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {
        bool isHawkSummoned = self.skillStack == 1;

        if (!isHawkSummoned)
        {
            self.StartSkill(new BattleSkillInstance_Hunter(self, null, false, attackBonusCoefficient * coefficient, 0f));
            return;
        }

        List<BattleCharacter> targets = GetTargets(self, allies, enemies, random, coefficient);

        if (targets.Count == 0)
            return;

        self.StartSkill(new BattleSkillInstance_Hunter(self, targets[0], true, 0f, sacrificeDamageCoefficient * coefficient));
    }
}

public class BattleSkillInstance_Hunter : BattleSkillInstance
{
    private readonly BattleCharacter target;
    private readonly bool isHawkSummoned;
    private readonly float attackBonusCoefficient;
    private readonly float sacrificeDamageCoefficient;

    public BattleSkillInstance_Hunter(BattleCharacter self, BattleCharacter target, bool isHawkSummoned, float attackBonusCoefficient, float sacrificeDamageCoefficient) : base(self, null, null, 0f, null)
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
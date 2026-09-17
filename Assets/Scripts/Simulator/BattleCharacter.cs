using System;
using System.Collections.Generic;

public class BattleCharacter
{
    public RuntimeCharacter runtimeCharacter;
    public BattleAIState aiState;
    public RuntimePlayer player;
    public CharacterBattleStatistics statistics;

    public CharacterSkill skill;

    // 현재 적용 중인 모디파이어
    private readonly Dictionary<CharacterStatType, List<BattleStatModifier>> modifiers = new();

    // 원본 스탯
    private readonly Dictionary<CharacterStatType, float> baseStats = new();

    // 모디파이어가 적용된 현재 스탯
    private readonly Dictionary<CharacterStatType, float> stats = new();

    // 도트데미지
    private readonly List<BattleDamageOverTime> damageOverTimes = new();

    public float currentHealth;
    public float currentMana;
    public float currentShield;

    // 실드 지속 시간
    public float shieldRemainingTime;

    public float position;

    public bool IsDead => currentHealth <= 0f;

    public float attackCooldown;
    public float actionLockTime;
    public float reactionTimer;
    public float skillDelayTimer;

    public bool isSkillReady;

    public BattleCharacter currentTarget;
    public float targetUpdateTimer;

    public bool CanAttack => attackCooldown <= 0f;
    public bool CanThink => reactionTimer <= 0f;
    public bool CanAct => actionLockTime <= 0f;

    public bool CanUseSkill => currentMana >= GetStat(CharacterStatType.MaxMana) && isSkillReady && skillDelayTimer <= 0f;

    public BattleCharacter() { }

    public BattleCharacter(RuntimeCharacter runtimeCharacter, RuntimePlayer player, BattleAIState ai, float startPosition)
    {
        Initialize(runtimeCharacter, player, ai, startPosition);
    }

    // ============================================================
    // Initialization
    // ============================================================

    public void Initialize(RuntimeCharacter runtimeCharacter, RuntimePlayer player, BattleAIState ai, float startPosition)
    {
        this.runtimeCharacter = runtimeCharacter;
        this.statistics = new CharacterBattleStatistics { runtimeCharacter = runtimeCharacter };

        this.player = player;
        this.skill = runtimeCharacter.OriginCharacter.skill;
        this.aiState = ai;

        InitializeStats();

        currentHealth = GetStat(CharacterStatType.Health);
        currentMana = 0f;
        currentShield = 0f;
        shieldRemainingTime = 0f;

        position = startPosition;

        attackCooldown = 0f;
        actionLockTime = 0f;
        reactionTimer = 0f;
        skillDelayTimer = 0f;

        isSkillReady = false;

        currentTarget = null;
        targetUpdateTimer = 0f;

        damageOverTimes.Clear();
    }

    private void InitializeStats()
    {
        foreach (CharacterStatType statType in Enum.GetValues(typeof(CharacterStatType)))
        {
            float value = runtimeCharacter.GetStat(statType);

            baseStats[statType] = value;
            stats[statType] = value;

            modifiers[statType] = new List<BattleStatModifier>();
        }
    }

    public void Reset()
    {
        currentMana = 0f;
        currentShield = 0f;
        shieldRemainingTime = 0f;

        attackCooldown = 0f;
        actionLockTime = 0f;
        reactionTimer = 0f;
        skillDelayTimer = 0f;

        isSkillReady = false;

        currentTarget = null;
        targetUpdateTimer = 0f;

        damageOverTimes.Clear();
    }

    // ============================================================
    // Stat
    // ============================================================

    public float GetStat(CharacterStatType statType)
    {
        if (stats.TryGetValue(statType, out float value))
            return value;

        return 0f;
    }

    // ============================================================
    // Modifier
    // ============================================================

    public void AddModifier(BattleStatModifier modifier)
    {
        if (!modifiers.TryGetValue(modifier.statType, out List<BattleStatModifier> list))
        {
            list = new List<BattleStatModifier>();
            modifiers.Add(modifier.statType, list);
        }

        list.Add(modifier);

        CalculateStat(modifier.statType);
    }

    public void RemoveModifier(BattleStatModifier modifier)
    {
        if (!modifiers.TryGetValue(modifier.statType, out List<BattleStatModifier> list))
            return;

        if (list.Remove(modifier))
        {
            CalculateStat(modifier.statType);
        }
    }

    // ============================================================
    // Modifier Tick
    // ============================================================

    public void TickModifiers()
    {
        foreach (var pair in modifiers)
        {
            CharacterStatType statType = pair.Key;
            List<BattleStatModifier> list = pair.Value;

            bool changed = false;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                BattleStatModifier modifier = list[i];

                modifier.remainingTicks--;

                if (modifier.remainingTicks <= 0)
                {
                    list.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed)
            {
                CalculateStat(statType);
            }
        }
    }

    // ============================================================
    // Shield
    // ============================================================

    public void AddShield(float amount, float duration)
    {
        if (amount <= 0f || duration <= 0f)
            return;

        currentShield += amount;
        shieldRemainingTime = duration;
    }

    public void TickShield(float tick)
    {
        if (currentShield <= 0f)
        {
            currentShield = 0f;
            shieldRemainingTime = 0f;
            return;
        }

        shieldRemainingTime -= tick;

        if (shieldRemainingTime <= 0f)
        {
            currentShield = 0f;
            shieldRemainingTime = 0f;
        }
    }

    // ============================================================
    // DoT Tick
    // ============================================================

    public void AddDamageOverTime(BattleDamageOverTime damageOverTime)
    {
        damageOverTimes.Add(damageOverTime);
    }

    public void TickDamageOverTimes(float tick)
    {
        for (int i = damageOverTimes.Count - 1; i >= 0; i--)
        {
            if (damageOverTimes[i].Tick(tick))
                damageOverTimes.RemoveAt(i);
        }
    }

    // ============================================================
    // Calculate
    // ============================================================

    private void CalculateStat(CharacterStatType statType)
    {
        if (!baseStats.TryGetValue(statType, out float baseValue))
            return;

        if (!modifiers.TryGetValue(statType, out List<BattleStatModifier> list))
        {
            stats[statType] = baseValue;
            return;
        }

        float percentModifier = 0f;
        float flatModifier = 0f;

        foreach (BattleStatModifier modifier in list)
        {
            switch (modifier.modifierType)
            {
                case BattleStatModifierType.Percent:
                    percentModifier += modifier.value;
                    break;

                case BattleStatModifierType.Flat:
                    flatModifier += modifier.value;
                    break;
            }
        }

        float result = baseValue * (1f + percentModifier) + flatModifier;

        stats[statType] = result;
    }
}

[Serializable]
public class CharacterBattleStatistics
{
    public RuntimeCharacter runtimeCharacter;

    public int attackCount;
    public int skillCount;

    public float damageDealt;
    public float damageTaken;

    public float moveDistance;

    public float survivalTime;

    public void Reset()
    {
        attackCount = 0;
        skillCount = 0;

        damageDealt = 0;
        damageTaken = 0;

        moveDistance = 0;

        survivalTime = 0;
    }
}
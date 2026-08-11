using System;

public class BattleCharacter
{
    public RuntimeCharacter runtimeCharacter;
    public BattleAIState aiState;
    public RuntimePlayer player;
    public CharacterBattleStatistics statistics;

    public CharacterSkill skill;

    public float currentHealth;
    public float currentMana;

    public float attack;
    public float defence;
    public float attackSpeed;
    public float moveSpeed;
    public float attackRange;
    public float maxMana;

    public float position;

    public bool IsDead => currentHealth <= 0;

    public float attackCooldown;
    public float actionLockTime;
    public float reactionTimer;
    public float skillDelayTimer;
    public bool isSkillReady;

    public bool CanAttack => attackCooldown <= 0f;
    public bool CanThink => reactionTimer <= 0f;
    public bool CanAct => actionLockTime <= 0f;
    public bool CanUseSkill => currentMana >= maxMana && isSkillReady && skillDelayTimer <= 0f;

    public BattleCharacter(RuntimeCharacter runtimeCharacter, RuntimePlayer player, BattleAIState ai, float startPosition)
    {
        this.runtimeCharacter = runtimeCharacter;
        this.statistics = new CharacterBattleStatistics { runtimeCharacter = runtimeCharacter };
        this.player = player;
        this.skill = runtimeCharacter.OriginCharacter.skill;
        aiState = ai;

        attack = runtimeCharacter.GetStat(CharacterStatType.Attack);
        defence = runtimeCharacter.GetStat(CharacterStatType.Defence);
        attackSpeed = runtimeCharacter.GetStat(CharacterStatType.AttackSpeed);
        moveSpeed = runtimeCharacter.GetStat(CharacterStatType.MoveSpeed);
        attackRange = runtimeCharacter.GetStat(CharacterStatType.AttackRange);
        maxMana = runtimeCharacter.GetStat(CharacterStatType.MaxMana);

        currentHealth = runtimeCharacter.GetStat(CharacterStatType.Health);

        position = startPosition;
        attackCooldown = 0;
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
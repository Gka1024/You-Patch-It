using System;
using UnityEngine;

public class BattleSimulator : MonoBehaviour
{
    public static BattleSimulator Instance;
    public PlayerProfile sampleProfile;

    public StatisticsManager statisticsManager;

    private const float tick = 0.05f;
    private const float BATTLE_TIME_LIMIT = 300f;

    private void Awake()
    {
        Instance = this;
    }

    public void StartSimulation()
    {
        RuntimeCharacter red = RuntimeCharacterManager.Instance.GetRuntimeCharacter(101);
        RuntimeCharacter blue = RuntimeCharacterManager.Instance.GetRuntimeCharacter(103);

        StartSimulation(red, blue, 10000, UnityEngine.Random.Range(1, int.MaxValue));

        SeasonManager.Instance.FinishSimulation();
    }

    public void StartSimulation(RuntimeCharacter redCharacter, RuntimeCharacter blueCharacter, int simulationCount, int simulationSeed)
    {
        Debug.Log($"{redCharacter.OriginCharacter.name} vs {blueCharacter.OriginCharacter.name} || Seed : {simulationSeed}");

        System.Random simulationRandom = new System.Random(simulationSeed);

        BattleStatistics statistics = new BattleStatistics();
        statistics.Register(redCharacter);
        statistics.Register(blueCharacter);

        for (int i = 0; i < simulationCount; i++)
        {
            int battleSeed = simulationRandom.Next();

            BattleResult result = Simulate(redCharacter, blueCharacter, battleSeed);
            statisticsManager.RecordBattle(result);
        }
    }

    public BattleResult Simulate(RuntimeCharacter redCharacter, RuntimeCharacter blueCharacter, int battleSeed)
    {
        System.Random battleRandom = new System.Random(battleSeed);

        BattleStatistics statistics = new BattleStatistics();
        statistics.Register(redCharacter);
        statistics.Register(blueCharacter);

        BattleAIState redAI = new BattleAIState(redCharacter.OriginCharacter.battleAI, sampleProfile, battleRandom);
        BattleCharacter red = new BattleCharacter(redCharacter, redAI, statistics.Get(redCharacter), 0f);

        BattleAIState blueAI = new BattleAIState(blueCharacter.OriginCharacter.battleAI, sampleProfile, battleRandom);
        BattleCharacter blue = new BattleCharacter(blueCharacter, blueAI, statistics.Get(blueCharacter), 10f);

        float battleTime = 0f;

        while (!red.IsDead && !blue.IsDead)
        {
            // Tick 감소
            TickCharacter(red);
            TickCharacter(blue);

            // 둘 다 행동을 결정
            BattleAction redAction = BattleAction.None;
            BattleAction blueAction = BattleAction.None;

            if (red.CanAct && red.CanThink)
            {
                red.reactionTimer = red.aiState.reactionTime;
                redAction = red.aiState.DecideAction(red, blue);
            }

            if (blue.CanAct && blue.CanThink)
            {
                blue.reactionTimer = blue.aiState.reactionTime;
                blueAction = blue.aiState.DecideAction(blue, red);
            }

            // 결정된 행동 실행
            ExecuteAction(red, blue, redAction, tick);
            ExecuteAction(blue, red, blueAction, tick);

            battleTime += tick;

            if (battleTime > BATTLE_TIME_LIMIT)
            {
                Debug.LogWarning($"Battle Timeout (Seed : {battleSeed})");
                break;
            }
        }

        statistics.battleDuration = battleTime;
        red.statistics.survivalTime = battleTime;
        blue.statistics.survivalTime = battleTime;

        return new BattleResult(
            red.IsDead ? blue.runtimeCharacter : red.runtimeCharacter,
            red.IsDead ? red.runtimeCharacter : blue.runtimeCharacter,
            statistics);
    }

    private void TickCharacter(BattleCharacter character)
    {
        character.attackCooldown = Mathf.Max(0, character.attackCooldown - tick);
        character.actionLockTime = Mathf.Max(0, character.actionLockTime - tick);
        character.reactionTimer = Mathf.Max(0, character.reactionTimer - tick);
    }

    private void ExecuteAction(BattleCharacter self, BattleCharacter enemy, BattleAction action, float tick)
    {
        switch (action)
        {
            case BattleAction.MoveTowards:
                MoveTowards(self, enemy, tick);
                break;

            case BattleAction.MoveAway:
                MoveAway(self, enemy, tick);
                break;

            case BattleAction.Attack:
                Attack(self, enemy);
                break;
        }
    }

    private void MoveTowards(BattleCharacter self, BattleCharacter enemy, float tick)
    {
        float direction = Mathf.Sign(enemy.position - self.position);
        Move(self, direction, tick);
    }

    private void MoveAway(BattleCharacter self, BattleCharacter enemy, float tick)
    {
        float direction = -Mathf.Sign(enemy.position - self.position);
        Move(self, direction, tick);
    }

    private void Move(BattleCharacter self, float direction, float tick)
    {
        float moveDistance = self.moveSpeed * tick;

        self.position += direction * moveDistance;

        self.statistics.moveDistance += moveDistance;
    }

    private void Attack(BattleCharacter self, BattleCharacter enemy)
    {
        float damage = Mathf.Max(1, self.attack * (100f / (100f + enemy.defence)));

        enemy.currentHealth -= damage;

        self.attackCooldown = 1f / self.attackSpeed;

        self.actionLockTime = 0.4f / self.attackSpeed;

        self.statistics.damageDealt += damage;
        enemy.statistics.damageTaken += damage;
    }
}

public enum BattleSide
{
    Red,
    Blue
}


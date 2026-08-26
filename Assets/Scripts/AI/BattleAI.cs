using System.Collections.Generic;
using UnityEngine;

public abstract class BattleAI : ScriptableObject
{
    [Header("Combat Style")]
    public float prefferedDistance;

    [System.Serializable]
    public class RoleScore
    {
        public CharacterRole role;
        public float score;
    }

    [SerializeField] private List<RoleScore> roleScoresList = new();
    public Dictionary<CharacterRole, float> roleScoresDictionary = new();

    // ==== Initialize

    protected virtual void OnEnable()
    {
        BuildRoleScoreDictionary();
    }

    private void BuildRoleScoreDictionary()
    {
        roleScoresDictionary = new Dictionary<CharacterRole, float>();

        foreach (RoleScore roleScore in roleScoresList)
        {
            roleScoresDictionary[roleScore.role] = roleScore.score;
        }
    }

    // ==== Decide Action

    public abstract BattleAction DecideAction(BattleCharacter self, BattleCharacter enemy, BattleAIState state);

    public virtual BattleCharacter GetTarget(BattleCharacter self, List<BattleCharacter> enemys, BattleAIState state)
    {
        BattleCharacter target = null;
        float highestScore = float.MinValue;

        foreach (BattleCharacter enemy in enemys)
        {
            if (enemy.IsDead)
                continue;

            float score = GetTargetScore(self, enemy, state);

            if (score > highestScore)
            {
                highestScore = score;
                target = enemy;
            }
        }

        return target;
    }

    // ==== Internal Calculation

    protected virtual float GetTargetScore(BattleCharacter self, BattleCharacter enemy, BattleAIState state)
    {
        float distance = GetDistance(self, enemy);
        float roleScore = GetRoleScore(enemy);
        float preferredDistance = GetPreferedDistance(self);

        if (distance > preferredDistance) return roleScore - (distance - preferredDistance) * 2f;

        return roleScore + (preferredDistance - distance) * 0.5f;
    }

    protected float GetDistance(BattleCharacter self, BattleCharacter enemy)
    {
        return Mathf.Abs(self.position - enemy.position);
    }

    protected float GetRoleScore(BattleCharacter enemy)
    {
        if (roleScoresDictionary.TryGetValue(enemy.runtimeCharacter.OriginCharacter.role, out float score))
            return score;

        return 0f;
    }

    protected float GetPreferedDistance(BattleCharacter self)
    {
        float attackRange = self.runtimeCharacter.GetStat(CharacterStatType.AttackRange);
        float moveSpeed = self.runtimeCharacter.GetStat(CharacterStatType.MoveSpeed);

        float approachTime = Mathf.Lerp(0.5f, 1.5f, self.player.RiskTaking / 100f);

        return attackRange + moveSpeed * approachTime;
    }

}
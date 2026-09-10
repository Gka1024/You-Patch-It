using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterSkill : ScriptableObject
{
    public int skillOwner;

    public string skillName;
    [TextArea] public string skillDescription;

    public bool ignoreDistance = false;

    public SkillTargetType targetType;
    public SkillTargetSelection targetSelection;

    public abstract void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random);

    protected List<BattleCharacter> GetTargets(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, System.Random random)
    {
        List<BattleCharacter> candidates = GetCandidates(allies, enemies);

        if (candidates.Count == 0)
            return candidates;

        switch (targetType)
        {
            case SkillTargetType.Self:
                return new List<BattleCharacter> { self };

            case SkillTargetType.SingleEnemy:
            case SkillTargetType.SingleAlly:
                return new List<BattleCharacter> { SelectTarget(self, candidates, random) };

            case SkillTargetType.AllEnemies:
            case SkillTargetType.AllAllies:
                return candidates;

            case SkillTargetType.RandomEnemy:
            case SkillTargetType.RandomAlly:
                return new List<BattleCharacter> { candidates[random.Next(candidates.Count)] };

            case SkillTargetType.AreaEnemy:
            case SkillTargetType.AreaAlly:
                return candidates;

            default:
                return new List<BattleCharacter>();
        }
    }

    private List<BattleCharacter> GetCandidates(List<BattleCharacter> allies, List<BattleCharacter> enemies)
    {
        switch (targetType)
        {
            case SkillTargetType.SingleEnemy:
            case SkillTargetType.AllEnemies:
            case SkillTargetType.AreaEnemy:
            case SkillTargetType.RandomEnemy:
                return GetAliveTargets(enemies);

            case SkillTargetType.SingleAlly:
            case SkillTargetType.AllAllies:
            case SkillTargetType.AreaAlly:
            case SkillTargetType.RandomAlly:
                return GetAliveTargets(allies);

            case SkillTargetType.Self:
                return new List<BattleCharacter>();

            default:
                return new List<BattleCharacter>();
        }
    }

    private List<BattleCharacter> GetAliveTargets(List<BattleCharacter> targets)
    {
        List<BattleCharacter> aliveTargets = new();

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null && !targets[i].IsDead)
                aliveTargets.Add(targets[i]);
        }

        return aliveTargets;
    }

    private BattleCharacter SelectTarget(BattleCharacter self, List<BattleCharacter> candidates, System.Random random)
    {
        switch (targetSelection)
        {
            case SkillTargetSelection.Nearest:
                return GetNearestTarget(self, candidates);

            case SkillTargetSelection.Farthest:
                return GetFarthestTarget(self, candidates);

            case SkillTargetSelection.LowestHealth:
                return GetLowestHealthTarget(candidates);

            case SkillTargetSelection.HighestHealth:
                return GetHighestHealthTarget(candidates);

            case SkillTargetSelection.LowestHealthPercent:
                return GetLowestHealthPercentTarget(candidates);

            case SkillTargetSelection.HighestHealthPercent:
                return GetHighestHealthPercentTarget(candidates);

            case SkillTargetSelection.Random:
                return candidates[random.Next(candidates.Count)];

            case SkillTargetSelection.Tank:
            case SkillTargetSelection.Support:
            case SkillTargetSelection.Assassin:
            case SkillTargetSelection.Ranged:
            case SkillTargetSelection.Mage:
            case SkillTargetSelection.Warrior:
                return GetRoleTarget(candidates, targetSelection);

            default:
                return candidates[0];
        }
    }

    private BattleCharacter GetNearestTarget(BattleCharacter self, List<BattleCharacter> candidates)
    {
        BattleCharacter target = candidates[0];
        float closestDistance = Mathf.Abs(self.position - target.position);

        for (int i = 1; i < candidates.Count; i++)
        {
            float distance = Mathf.Abs(self.position - candidates[i].position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = candidates[i];
            }
        }

        return target;
    }

    private BattleCharacter GetFarthestTarget(BattleCharacter self, List<BattleCharacter> candidates)
    {
        BattleCharacter target = candidates[0];
        float farthestDistance = Mathf.Abs(self.position - target.position);

        for (int i = 1; i < candidates.Count; i++)
        {
            float distance = Mathf.Abs(self.position - candidates[i].position);

            if (distance > farthestDistance)
            {
                farthestDistance = distance;
                target = candidates[i];
            }
        }

        return target;
    }

    private BattleCharacter GetLowestHealthTarget(List<BattleCharacter> candidates)
    {
        BattleCharacter target = candidates[0];

        for (int i = 1; i < candidates.Count; i++)
        {
            if (candidates[i].currentHealth < target.currentHealth)
                target = candidates[i];
        }

        return target;
    }

    private BattleCharacter GetHighestHealthTarget(List<BattleCharacter> candidates)
    {
        BattleCharacter target = candidates[0];

        for (int i = 1; i < candidates.Count; i++)
        {
            if (candidates[i].currentHealth > target.currentHealth)
                target = candidates[i];
        }

        return target;
    }

    private BattleCharacter GetLowestHealthPercentTarget(List<BattleCharacter> candidates)
    {
        BattleCharacter target = candidates[0];
        float targetPercent = GetHealthPercent(target);

        for (int i = 1; i < candidates.Count; i++)
        {
            float percent = GetHealthPercent(candidates[i]);

            if (percent < targetPercent)
            {
                targetPercent = percent;
                target = candidates[i];
            }
        }

        return target;
    }

    private BattleCharacter GetHighestHealthPercentTarget(List<BattleCharacter> candidates)
    {
        BattleCharacter target = candidates[0];
        float targetPercent = GetHealthPercent(target);

        for (int i = 1; i < candidates.Count; i++)
        {
            float percent = GetHealthPercent(candidates[i]);

            if (percent > targetPercent)
            {
                targetPercent = percent;
                target = candidates[i];
            }
        }

        return target;
    }

    private BattleCharacter GetRoleTarget(List<BattleCharacter> candidates, SkillTargetSelection selection)
    {
        CharacterRole role;

        switch (selection)
        {
            case SkillTargetSelection.Tank:
                role = CharacterRole.Tank;
                break;

            case SkillTargetSelection.Support:
                role = CharacterRole.Support;
                break;

            case SkillTargetSelection.Assassin:
                role = CharacterRole.Assassin;
                break;

            case SkillTargetSelection.Ranged:
                role = CharacterRole.Ranged;
                break;

            case SkillTargetSelection.Mage:
                role = CharacterRole.Mage;
                break;

            case SkillTargetSelection.Warrior:
                role = CharacterRole.Warrior;
                break;

            default:
                return candidates[0];
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].runtimeCharacter.OriginCharacter.role == role)
                return candidates[i];
        }

        return candidates[0];
    }

    private float GetHealthPercent(BattleCharacter target)
    {
        float maxHealth = target.GetStat(CharacterStatType.Health);

        if (maxHealth <= 0f)
            return 0f;

        return target.currentHealth / maxHealth;
    }
}

public enum SkillTargetType
{
    Self,
    SingleEnemy,
    SingleAlly,
    AllEnemies,
    AllAllies,
    AreaEnemy,
    AreaAlly,
    RandomEnemy,
    RandomAlly
}

public enum SkillTargetSelection
{
    Default,
    Nearest,
    Farthest,
    LowestHealth,
    HighestHealth,
    LowestHealthPercent,
    HighestHealthPercent,
    Random,
    Tank,
    Support,
    Assassin,
    Ranged,
    Mage,
    Warrior
}
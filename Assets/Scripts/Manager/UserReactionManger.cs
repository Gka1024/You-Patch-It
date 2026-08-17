using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserReactionManager : MonoBehaviour
{
    public static UserReactionManager Instance { get; private set; }

    [SerializeField] private TextAsset reactionJson;
    [SerializeField] private TextAsset prettyReactionJson;

    private List<UserReaction> reactions = new();
    private List<UserReaction> prettyReactions = new();

    private System.Random random;

    private void Awake()
    {
        Instance = this;

        Initialize();
    }

    private void Initialize()
    {
        if (reactionJson == null)
        {
            Debug.LogError("UserReactionManager : Reaction JSON이 등록되지 않았습니다.");
            return;
        }

        UserReactionDatabase database = JsonUtility.FromJson<UserReactionDatabase>(reactionJson.text);
        UserReactionDatabase database_pretty = JsonUtility.FromJson<UserReactionDatabase>(prettyReactionJson.text);

        reactions = database.reactions?.ToList() ?? new List<UserReaction>();
        prettyReactions = database_pretty.reactions?.ToList() ?? new List<UserReaction>();

        random = new System.Random();
    }

    public string GetReaction(RuntimeCharacter character, UserReactionCategory category, bool isPretty = false)
    {
        if (character == null)
            return string.Empty;

        List<UserReaction> reactionPool = isPretty ? prettyReactions : reactions;

        List<UserReaction> candidates = reactionPool.Where(x => x.id / 100 == (int)category).ToList();

        if (candidates.Count == 0)
        {
            Debug.LogWarning(
                $"UserReactionManager : Category {category}에 해당하는 반응이 없습니다.");

            return string.Empty;
        }

        UserReaction reaction =
            candidates[random.Next(candidates.Count)];

        return reaction.value.Replace(
            "{CharacterName}",
            character.OriginCharacter.characterName);
    }

    public UserReactionCategory GetCategory(CharacterTier tier)
    {
        int num = Random.Range(0, 100);

        switch (tier)
        {
            case CharacterTier.S: return num <= 10 ? UserReactionCategory.Negative : num <= 30 ? UserReactionCategory.Neutral : UserReactionCategory.Positive; // (1 : 2: 7)
            case CharacterTier.A: return num <= 20 ? UserReactionCategory.Negative : num <= 50 ? UserReactionCategory.Neutral : UserReactionCategory.Positive; // (2 : 3: 5)
            case CharacterTier.B: return num <= 30 ? UserReactionCategory.Negative : num <= 70 ? UserReactionCategory.Neutral : UserReactionCategory.Positive; // (3 : 4 : 3)
            case CharacterTier.C: return num <= 60 ? UserReactionCategory.Negative : num <= 90 ? UserReactionCategory.Neutral : UserReactionCategory.Positive; // (6 : 3 : 1)
            case CharacterTier.D: return num <= 70 ? UserReactionCategory.Negative : UserReactionCategory.Neutral; // 7: 3 : 0
        }

        return UserReactionCategory.Unset;
    }

}

public enum UserReactionCategory
{
    Unset = 0,
    Positive = 1,
    Negative = 2,
    Neutral = 3,
    LowPickRate = 4,
    HighPickRate = 5
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private List<Character> InGameCharacters = new();

    private Dictionary<int, Character> characterMap;

    /// <summary>
    /// 게임 시작 시 호출
    /// </summary>
    public void Initialize()
    {
        characterMap = new Dictionary<int, Character>();

        foreach (Character character in InGameCharacters)
        {
            if (character == null)
                continue;

            if (characterMap.ContainsKey(character.id))
            {
                Debug.LogError($"Character ID 중복 : {character.id}");
                continue;
            }

            characterMap.Add(character.id, character);
        }
    }

    public Character GetCharacter(int id)
    {
        if (characterMap == null)
            Initialize();

        characterMap.TryGetValue(id, out Character character);

        return character;
    }

    public IReadOnlyList<Character> GetAllCharacters()
    {
        return InGameCharacters;
    }

    public Character GetRandomCharacter()
    {
        return InGameCharacters[Random.Range(0, Count)];
    }

    public int Count => InGameCharacters.Count;
}
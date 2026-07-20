using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField]
    private List<Character> characters = new();

    private Dictionary<int, Character> characterMap;

    /// <summary>
    /// 게임 시작 시 호출
    /// </summary>
    public void Initialize()
    {
        characterMap = new Dictionary<int, Character>();

        foreach (Character character in characters)
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
        return characters;
    }

    public Character GetRandomCharacter()
    {
        return characters[Random.Range(0, Count)];
    }

    public int Count => characters.Count;
}
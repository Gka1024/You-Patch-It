using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;

public class RuntimeCharacterManager : MonoBehaviour
{
    public static RuntimeCharacterManager Instance;

    [SerializeField] private Dictionary<int, RuntimeCharacter> runtimeCharacters = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }
    }

    public void Initialize(CharacterDatabase database)
    {
        runtimeCharacters.Clear();

        foreach (Character character in database.GetAllCharacters())
        {
            RuntimeCharacter runtimeCharacter = new RuntimeCharacter(character);
            runtimeCharacters.Add(character.id, runtimeCharacter);
        }
    }

    public void RegisterRuntimeCharacter(RuntimeCharacter character)
    {
        runtimeCharacters.Add(character.OriginCharacter.id, character);
    }

    public RuntimeCharacter GetRuntimeCharacter(int id)
    {
        runtimeCharacters.TryGetValue(id, out RuntimeCharacter character);

        return character;
    }

    public List<RuntimeCharacter> GetCharactersInRole(CharacterRole role)
    {
        List<RuntimeCharacter> result = new();

        foreach(var kvp in runtimeCharacters)
        {
            if(kvp.Value.OriginCharacter.role == role)
            {
                result.Add(kvp.Value);
            }
        }

        return result;
    }

    public IEnumerable<RuntimeCharacter> GetAllCharacters()
    {
        return runtimeCharacters.Values;
    }
}
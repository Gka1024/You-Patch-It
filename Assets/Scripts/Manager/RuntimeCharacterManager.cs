using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RuntimeCharacterManager : MonoBehaviour
{
    public static RuntimeCharacterManager Instance;
    [SerializeField] private CharacterDatabase characterDatabase;

    [SerializeField] private List<int> startingCharacterIds = new();
    [SerializeField] private List<int> primalAdditionalCharacterIds = new();

    private readonly HashSet<Character> lockedCharacters = new();
    public int LockedCharacterCount => lockedCharacters.Count;

    [SerializeField] private Dictionary<int, RuntimeCharacter> runtimeCharacters = new();
    public int CharacterCount => runtimeCharacters.Count;

    public RuntimeCharacter AddedRuntimeCharacter;

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
        characterDatabase = database;

        runtimeCharacters.Clear();
        lockedCharacters.Clear();

        foreach (Character character in database.GetAllCharacters())
        {
            if (startingCharacterIds.Contains(character.id))
            {
                AddRuntimeCharacter(character.id);
            }
            else if(primalAdditionalCharacterIds.Contains(character.id))
            {
                continue;
            }
            else
            {
                lockedCharacters.Add(character);
            }
        }
    }

    public RuntimeCharacter AddRandomCharacter(System.Random random)
    {
        if (primalAdditionalCharacterIds.Count > 0)
        {
            RuntimeCharacter character = AddRuntimeCharacter(primalAdditionalCharacterIds[0]);
            primalAdditionalCharacterIds.RemoveAt(0);
            return character;
        }

        if (lockedCharacters.Count == 0)
        {
            AddedRuntimeCharacter = null;
            return null;
        }

        int index = random.Next(lockedCharacters.Count);

        Character selected = lockedCharacters.ElementAt(index);

        lockedCharacters.Remove(selected);

        return AddRuntimeCharacter(selected.id, true);
    }

    private RuntimeCharacter AddRuntimeCharacter(int id, bool Showlog = false)
    {
        RuntimeCharacter runtime = new RuntimeCharacter(characterDatabase.GetCharacter(id));

        runtimeCharacters.Add(id, runtime);
        AddedRuntimeCharacter = runtime;

        if (Showlog) Debug.Log($"새 캐릭터 추가 : {runtime.OriginCharacter.characterName}");

        return runtime;
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

    public bool HasLockedCharacter()
    {
        return lockedCharacters.Count > 0;
    }

    public List<RuntimeCharacter> GetCharactersInRole(CharacterRole role)
    {
        List<RuntimeCharacter> result = new();

        foreach (var kvp in runtimeCharacters)
        {
            if (kvp.Value.OriginCharacter.role == role)
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

    public RuntimeCharacter GetRandomCharacter()
    {
        if (runtimeCharacters.Count == 0)
            return null;

        List<RuntimeCharacter> characters = runtimeCharacters.Values.ToList();

        return characters[Random.Range(0, characters.Count)];
    }
}
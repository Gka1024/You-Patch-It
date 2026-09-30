using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterPreviewWorld : MonoBehaviour
{
    [SerializeField] private Transform characterParent;

    private Dictionary<int, GameObject> characterInstances = new Dictionary<int, GameObject>();

    private GameObject currentCharacter;

    public void ShowCharacter(Character character)
    {
        // 기존 캐릭터 비활성화
        ResetAllCharacters();

        if (character == null || character.CharacterPrefab_SPUM == null)
        {
            // Debug.Log($"{character.characterName} - Has SPUM? : {character.CharacterPrefab_SPUM != null}");
            return;
        }

        // 이미 생성된 캐릭터인지 확인
        if (characterInstances.TryGetValue(character.id, out GameObject instance))
        {
            currentCharacter = instance;
            currentCharacter.SetActive(true);
            return;
        }

        // 최초 선택 시 생성
        SpawnNewCharacter(character);
    }

    private void ResetAllCharacters()
    {
        foreach (Transform trs in characterParent)
        {
            trs.gameObject.SetActive(false);
        }
    }

    private void SpawnNewCharacter(Character character)
    {
        GameObject newCharacter = Instantiate(character.CharacterPrefab_SPUM, new Vector3(0, -150, 0), quaternion.identity, characterParent);

        newCharacter.transform.localScale = new Vector3(400, 400, 400);

        characterInstances.Add(character.id, newCharacter);

        currentCharacter = newCharacter;
    }
}
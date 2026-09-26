using UnityEngine;

public class CharacterPreviewUI : MonoBehaviour
{
    public GameObject character_SPUM;

    public void SetCharacter(Character character)
    {
        character_SPUM = character.CharacterPreview_SPUM;
    }
}
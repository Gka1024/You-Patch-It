using UnityEngine;

public class CharacterPreviewUI : MonoBehaviour
{
    [SerializeField] private  CharacterPreviewWorld characterPreviewWorld;

    public void SetCharacter(Character character)
    {
        characterPreviewWorld.ShowCharacter(character);
    }

}
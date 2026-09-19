using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomSkillDescriptionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text skillName;
    [SerializeField] private TMP_Text skillDescription;
    [SerializeField] private Image RoleImage;

    public void Initialize(Character character, Image image)
    {
        characterName.text = character.characterName + " : " + character.skill.skillName;

        if (character.skill != null)
        {
            skillDescription.text = character.skill.skillDescription;
        }

        RoleImage.sprite = image.sprite;
    }
}
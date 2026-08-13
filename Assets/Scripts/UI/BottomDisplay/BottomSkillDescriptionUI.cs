using TMPro;
using UnityEngine;

public class BottomSkillDescriptionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text skillName;
    [SerializeField] private TMP_Text skillDescription;

    public void SetText(Character character)
    {
        characterName.text = character.characterName;

        if (character.skill != null)
        {
            skillDescription.text = character.skill.skillName + " : " + character.skill.skillDescription;
        }
    }
}
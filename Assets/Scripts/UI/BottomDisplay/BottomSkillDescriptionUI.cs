using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomSkillDescriptionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text skillName;
    [SerializeField] private TMP_Text skillDescription;
    [SerializeField] private Image RoleImage;
    [SerializeField] private DescriptionPopupUI PopupSkillDescription;

    public void Initialize(Character character, Image image)
    {
        if (character == null || character.skill == null)
            return;

        characterName.text = character.characterName;
        skillDescription.text = character.skill.skillDescription;

        RoleImage.sprite = image.sprite;

        // 툴팁에 표시할 효과 정보 생성
        RuntimeCharacter runtimeCharacter = RuntimeCharacterManager.Instance.GetRuntimeCharacter(character);
        List<SkillEffectInfo> effects = character.skill.GetEffectInfo(runtimeCharacter);

        StringBuilder description = new StringBuilder();

        foreach (SkillEffectInfo effect in effects)
        {
            description.AppendLine($"{effect.effectName} : {effect.value:0.##}{effect.unit}");
        }

        PopupSkillDescription.SetText("", description.ToString());
    }
}
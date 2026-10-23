
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterToggleUI : MonoBehaviour
{
    public RuntimeCharacter Character { get; private set; }

    [SerializeField] private TMP_Text CharacterName;
    [SerializeField] private Toggle characterToggle;
    [SerializeField] private Image characterColorImage;

    public event Action<RuntimeCharacter, bool> OnToggleChanged;

    private void Awake()
    {
        if (characterToggle != null)
            characterToggle.onValueChanged.AddListener(HandleToggleChanged);
    }

    private void OnDestroy()
    {
        if (characterToggle != null)
            characterToggle.onValueChanged.RemoveListener(HandleToggleChanged);
    }

    public void Initialize(RuntimeCharacter character, Color color)
    {
        Character = character;

        if (Character == null || Character.OriginCharacter == null)
            return;

        SetName(Character.OriginCharacter.characterName);
        SetColor(color);
    }

    public void SetName(string characterName)
    {
        if (CharacterName != null)
            CharacterName.SetText(characterName);
    }

    public void SetColor(Color color)
    {
        if (characterColorImage != null)
        {
            characterColorImage.color = color;
            CharacterName.color = color;
        }
    }

    public void SetIsOnWithoutNotify(bool isOn)
    {
        if (characterToggle != null)
            characterToggle.SetIsOnWithoutNotify(isOn);
    }

    public bool IsOn()
    {
        return characterToggle != null && characterToggle.isOn;
    }

    private void HandleToggleChanged(bool isOn)
    {
        if (Character != null)
            OnToggleChanged?.Invoke(Character, isOn);
    }
}

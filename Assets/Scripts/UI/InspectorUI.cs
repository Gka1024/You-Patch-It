using TMPro;
using UnityEngine;

public class InspectorUI : MonoBehaviour
{
    public static InspectorUI Instance;

    [SerializeField] private TMP_Text nameText;
    private RuntimeCharacter currentCharacter;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(RuntimeCharacter character)
    {
        currentCharacter = character;
        Refresh();
    }

    public void Refresh()
    {
        if(currentCharacter == null)
        {
            return;
        }

        nameText.text = currentCharacter.OriginCharacter.characterName;
    }
}
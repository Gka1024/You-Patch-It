using TMPro;
using UnityEngine;

public class SpecificPatchUI : MonoBehaviour
{
    [SerializeField] private TMP_Text item;
    [SerializeField] private TMP_Text beforeText;
    [SerializeField] private TMP_Text arrowText;
    [SerializeField] private TMP_Text afterText;


    public void Initialize(CharacterStatType stat, float before, float after)
    {
        Debug.Log("SpecificPatchUI Initialize");
        SetText(stat, before, after);
    }

    private void SetText(CharacterStatType stat, float before, float after)
    {
        item.text = stat.ToString();
        beforeText.text = $"{before}";
        afterText.text = $"{after}";
    }

}
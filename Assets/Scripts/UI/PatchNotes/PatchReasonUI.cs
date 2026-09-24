using TMPro;
using UnityEngine;

public class PatchReasonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    public void Initialize(string description)
    {
        text.text = description;
    }
}
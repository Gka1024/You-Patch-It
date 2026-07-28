using TMPro;
using UnityEngine;

public class PatchReasonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    public void Initialize(PatchReason reason)
    {
        text.text = DisplayNameHelper.GetReasonName(reason);
    }
}
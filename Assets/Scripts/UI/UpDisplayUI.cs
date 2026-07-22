using TMPro;
using UnityEngine;

public class UpDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text SeasonText;
    [SerializeField] private TMP_Text TrustPointText;
    [SerializeField] private TMP_Text DevelopResourceText;

    public void Refresh()
    {
        Debug.Log("UpdisplayUI refresh");
        SeasonText.text = $"시즌 {SeasonManager.Instance.CurrentSeason} - {SeasonManager.Instance.CurrentSubSeason}";
        TrustPointText.text = $"{ResourceManager.Instance.TrustPoint}%";
        DevelopResourceText.text = $"{ResourceManager.Instance.DevelopResource}";
    }


}

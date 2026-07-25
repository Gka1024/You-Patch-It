using TMPro;
using UnityEngine;

public class UpDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text SeasonText;
    [SerializeField] private TMP_Text TrustPointText;
    [SerializeField] private TMP_Text DevelopResourceText;

    public GameObject GameOverUI;

    public void Refresh()
    {
        Debug.Log("UpdisplayUI refresh");
        SeasonText.text = $"시즌 {SeasonManager.Instance.DisplaySeason} - {SeasonManager.Instance.DisplaySubSeason}";
        TrustPointText.text = $"{ResourceManager.Instance.TrustPoint}%";
        DevelopResourceText.text = $"{ResourceManager.Instance.DevelopResource}";
    }


}

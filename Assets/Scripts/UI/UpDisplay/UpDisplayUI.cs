using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text SeasonText;
    [SerializeField] private TMP_Text TrustPointText;
    [SerializeField] private TMP_Text DevelopResourceText;
    [SerializeField] private TMP_Text PlayersCountText;

    [SerializeField] private Button ExitButton;
    [SerializeField] private GameObject ExitPopup;

    public GameObject GameOverUI;

    void Awake()
    {
        ExitButton.onClick.AddListener(ShowExitPopup);
    }

    public void Refresh()
    {
        Debug.Log("UpdisplayUI refresh");
        SeasonText.text = $"시즌 데이터 : {SeasonManager.Instance.DisplaySeason} - {SeasonManager.Instance.DisplaySubSeason}";
        TrustPointText.text = $"{ResourceManager.Instance.TrustPoint}%";
        DevelopResourceText.text = $"{ResourceManager.Instance.DevelopResource}";
        PlayersCountText.text = $"{PlayerManager.Instance.GetCurrentPlayer}";
    }

    private void ShowExitPopup()
    {
        ExitPopup.SetActive(true);
    }
}

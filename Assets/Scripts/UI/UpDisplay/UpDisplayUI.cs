using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text SeasonText;
    [SerializeField] private TMP_Text TrustPointText;
    [SerializeField] private TMP_Text DevelopResourceText;
    [SerializeField] private TMP_Text PlayersCountText;

    [SerializeField] private GameObject TrustText;
    [SerializeField] private GameObject ResourceText;

    [SerializeField] private Button ExitButton;
    [SerializeField] private GameObject ExitPopup;

    public GameObject GameOverUI;

    void Awake()
    {
        ExitButton.onClick.AddListener(ShowExitPopup);
    }

    void Start()
    {
        UnlockManager.Instance.OnUnlockChanged += Refresh;
    }

    public void Refresh()
    {
        Debug.Log("UpdisplayUI refresh");
        SeasonText.text = $"시즌 데이터 : {SeasonManager.Instance.DisplaySeason} - {SeasonManager.Instance.DisplaySubSeason}";
        TrustPointText.text = $"{ResourceManager.Instance.TrustPoint}%";
        DevelopResourceText.text = $"{ResourceManager.Instance.DevelopResource}";
        PlayersCountText.text = $"{PlayerManager.Instance.GetCurrentPlayer}";

        ResetResourceTextDescription();
    }

    private void ResetResourceTextDescription()
    {
        ResourceText.GetComponent<DescriptionPopupUI>().SetText(
            "리소스",
            $"개발 및 조정에 들어가는 \n재화입니다. \n캐릭터 패치에 들어가는 \n리소스 : {PatchManager.Instance.GetRequiredResource()}"
        );

    }

    private void ShowExitPopup()
    {
        ExitPopup.SetActive(true);
    }
}

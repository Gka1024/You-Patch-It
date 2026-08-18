using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public DashBoardUI dashBoardUI;
    public UpDisplayUI upDisplayUI;
    public BottomDisplayUI bottomDisplayUI;

    public CharacterTableUI characterTableUI;
    public InspectorUI inspectorUI;
    public PatchReasonPopupUI patchReasonPopupUI;
    public GameObject ShowLackResource;

    public DeveloperGoalUI developerGoalUI;

    public PatchNoteUI patchNoteUI;

    public SeasonReportUI seasonReportUI;

    public GameObject GameOverText;
    public GameObject GoalUnsetAlert;

    public GameObject InstantDescription;


    void Awake()
    {
        Instance = this;
    }

    public GameObject SpawnInstantDesc(string name, string desc)
    {
        InstantDescription.SetActive(true);

        InstantDescriptionUI descriptionUI =
            InstantDescription.GetComponent<InstantDescriptionUI>();

        descriptionUI.Initialize(name, desc);
        descriptionUI.SetPosition(Input.mousePosition);

        return InstantDescription;
    }

    public void DespawnInstantDesc()
    {
        InstantDescription.SetActive(false);
    }
}
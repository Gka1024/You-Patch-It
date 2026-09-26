using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Base Tabs")]
    public DashBoardUI dashBoardUI;
    public UpDisplayUI upDisplayUI;
    public BottomDisplayUI bottomDisplayUI;
    public CharacterTableUI characterTableUI;
    public InspectorUI inspectorUI;

    [Header("Additional Tabs")]
    public PatchNoteUI patchNoteUI;
    public EventUI eventUI;
     public SeasonReportUI seasonReportUI;
    public DeveloperGoalUI developerGoalUI;
   
    [Header("PopUps")]
    public GameObject InstantDescription;
    public EncounterPopupUI encounterPopupUI;
    public CharacterPreviewUI characterPreviewPopupUI;

    [Header("TextAlerts")]
    public GameObject ShowLackResourceAlert;
    public GameObject GameOverText;
    public GameObject GoalUnsetAlert;

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
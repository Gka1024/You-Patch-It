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


    void Awake()
    {
        Instance = this;
    }
}
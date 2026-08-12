using UnityEngine;

public class UIManager: MonoBehaviour
{
    public static UIManager Instance;

    public DashBoardUI dashBoardUI;
    public UpDisplayUI upDisplayUI;

    public CharacterTableUI characterTableUI;

    public InspectorUI inspectorUI;
    public PatchReasonPopupUI patchReasonPopupUI;
    
    public DeveloperGoalUI developerGoalUI;

    public PatchNoteUI patchNoteUI;

    public GameObject GameOverUI;
    public GameObject GoalUnsetAlert;


    void Awake()
    {
        Instance = this;
    }
}
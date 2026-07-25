using UnityEngine;

public class UIManager: MonoBehaviour
{
    public static UIManager Instance;

    public DashBoardUI dashBoardUI;
    public UpDisplayUI upDisplayUI;

    public CharacterTableUI characterTableUI;
    public CharacterTableHeaderUI characterTableHeaderUI;
    public CharacterRowUI characterRowUI;

    public InspectorUI inspectorUI;
    public InspectorWinrateUI inspectorWinrateUI;
    public InspectorRowUI inspectorRowUI;
    public InspectorAnalysisRowUI inspectorAnalysisRowUI;
    public PatchReasonPopupUI patchReasonPopupUI;
    public PatchReasonToggleUI patchReasonToggleUI;
    
    public DeveloperGoalUI developerGoalUI;
    public DeveloperGoalItemUI developerGoalItemUI;

    public GameObject GameOverUI;

    void Awake()
    {
        Instance = this;
    }
}
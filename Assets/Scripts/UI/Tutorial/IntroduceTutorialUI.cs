public class IntroduceTutorialUI : TutorialIndexUI
{
    protected override void OnSeasonProceeded()
    {
        switch (index)
        {
            case 2:
                UIManager.Instance.dashBoardUI.ShowCharacter();
                break;

            case 3:
                UIManager.Instance.dashBoardUI.ShowPatchNote();
                break;

            case 4:
                UIManager.Instance.dashBoardUI.ShowUnlock();
                break;

            case 5:
                UIManager.Instance.dashBoardUI.ShowGoals();
                break;

            case 6:
                UIManager.Instance.dashBoardUI.ShowSeasonReports();
                break;

            case 7:
                UIManager.Instance.dashBoardUI.ShowCharacter();
                break;

            default: break;
        }
    }
}
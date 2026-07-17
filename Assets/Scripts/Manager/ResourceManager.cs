using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void GiveReward()
    {
        Debug.Log("GiveReward Start");
        SeasonManager.Instance.FinishReward();
    }

    public void ConfirmPatch()
    {
        SeasonManager.Instance.FinishPatch();
    }
}
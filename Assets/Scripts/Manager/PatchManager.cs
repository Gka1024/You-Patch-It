using UnityEngine;

public class PatchManager : MonoBehaviour
{
    public static PatchManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void StartPatch()
    {
        Debug.Log("Patch Start");
    }

    public void ConfirmPatch()
    {
        SeasonManager.Instance.FinishPatch();
    }
}
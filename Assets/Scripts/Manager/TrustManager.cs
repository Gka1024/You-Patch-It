using UnityEngine;

public class TrustManager : MonoBehaviour
{
    public static TrustManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CalculateTrust()
    {
        Debug.Log("Calculate Start");
    }

    public void ConfirmPatch()
    {
        SeasonManager.Instance.FinishPatch();
    }
}
using UnityEngine;

public class TrustManager : MonoBehaviour
{
    public static TrustManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CalculateTrust()
    { // 신뢰도에 비례해서 유저 수 증감
        Debug.Log("Calculate Start");
    }

    public void ConfirmPatch()
    {
        SeasonManager.Instance.FinishPatch();
    }
}
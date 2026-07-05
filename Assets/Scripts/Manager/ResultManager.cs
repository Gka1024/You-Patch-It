using UnityEngine;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateResult()
    {
        Debug.Log("Result");

        SeasonManager.Instance.FinishResult();
    }
}
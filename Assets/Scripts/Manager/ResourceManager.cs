using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Resource")]
    [SerializeField] private int developResource;

    [Header("Trust")]
    [SerializeField]
    [Range(0, 100)]
    private float trust = 50f;

    public int TrustPoint => Mathf.RoundToInt(trust);
    public int DevelopResource => developResource;

    public float curSeasonTrust;
    public int curSeasonResource;

    private const int REDUCE_TRUST_DECREASE_I = 3011;
    private const int REDUCE_TRUST_DECREASE_II = 3012;

    private void Awake()
    {
        Instance = this;

        trust = 50f;
        developResource = 300;
    }

    public void ResetCurrentSeason()
    {
        curSeasonResource = 0;
        curSeasonTrust = 0;
    }

    //====================================================
    // Reward
    //====================================================

    public void CheckReward()
    {
        if (trust <= 0)
        {
            GameManager.Instance.GameOver();
        }
    }

    public void GiveSeasonReward(int develop, float trustPoint)
    {
        curSeasonResource += AddDevelopResource(develop);
        curSeasonTrust += AddTrust(trustPoint);

        UIManager.Instance.upDisplayUI.Refresh();
    }

    public void AddReward(GoalReward reward)
    {
        curSeasonResource += AddDevelopResource(reward.DevelopResource);
        curSeasonTrust += AddTrust(reward.TrustPoint);

        UIManager.Instance.upDisplayUI.Refresh();
    }

    //====================================================
    // Trust
    //====================================================

    public float AddTrust(float amount)
    {
        if (amount >= 0)
        {
            float multiplier = Mathf.Pow((100f - trust) / 100f, 1.5f);
            trust += amount * multiplier;
        }
        else
        {
            float value = amount;
            value += UnlockManager.Instance.IsUnlocked(REDUCE_TRUST_DECREASE_I) ?
            UnlockManager.Instance.IsUnlocked(REDUCE_TRUST_DECREASE_II) ? 0.8f : 0.9f : 1.0f;

            trust += value;
        }

        trust = Mathf.Clamp(trust, 0, 100);

        if (trust <= 0)
        {
            Debug.Log("Game Over");
            // TODO : GameOver
        }

        return amount;
    }

    //====================================================
    // Develop Resource
    //====================================================

    public int AddDevelopResource(int amount)
    {
        developResource += Mathf.Max(0, amount);
        return amount;
    }

    public bool SpendDevelopResource(int amount)
    {
        if (developResource < amount)
            return false;

        developResource -= amount;

        UIManager.Instance.upDisplayUI.Refresh();

        return true;
    }
}
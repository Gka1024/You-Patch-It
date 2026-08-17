using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonReportUI : MonoBehaviour
{
    public static SeasonReportUI Instance;

    [SerializeField] private List<GameObject> CharacterRows;
    [SerializeField] private GameObject CharacterRowParent;
    [SerializeField] private GameObject CharacterRowPrefab;

    [SerializeField] private TMP_Text TrustPoint;
    [SerializeField] private TMP_Text TrustPointText;

    [SerializeField] private TMP_Text ResourcePoint;
    [SerializeField] private TMP_Text ResourcePointText;
    [SerializeField] private Button ProceedButton;
    public bool IsSeasonFinished;

    void Awake()
    {
        Instance = this;
        IsSeasonFinished = false;
        ProceedButton.onClick.AddListener(ProceedSeason);
    }

    public void Initialize(int currentSeason)
    {
        IsSeasonFinished = true;

        for (int i = CharacterRowParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(CharacterRowParent.transform.GetChild(i).gameObject);
        }

        CharacterRows.Clear();

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            SeasonReportRowUI row = Instantiate(CharacterRowPrefab, CharacterRowParent.transform).GetComponent<SeasonReportRowUI>();
            List<CharacterStatistics> stats = StatisticsManager.Instance.GetSeasonStatistics(character.OriginCharacter.id, currentSeason);
            row.Initialize(character, stats);
            CharacterRows.Add(row.gameObject);
        }

        RuntimeCharacter addCharacter = RuntimeCharacterManager.Instance.AddedRuntimeCharacter;

        TrustPointText.text = "+ " + ResourceManager.Instance.curSeasonTrust.ToString();
        ResourcePointText.text = "+ " + ResourceManager.Instance.curSeasonResource.ToString();
    }


    private void ProceedSeason()
    {
        if (IsSeasonFinished)
        {
            IsSeasonFinished = false;
            SeasonManager.Instance.NextSeason();
        }
    }
}

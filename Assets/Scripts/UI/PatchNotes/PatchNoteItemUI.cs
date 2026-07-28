using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatchNoteItemUI : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private Button itemButton;
    [SerializeField] private TMP_Text arrow;

    [SerializeField] private TMP_Text currentSeasonText;
    [SerializeField] private TMP_Text seasonWinrate;
    [SerializeField] private TMP_Text seasonPickrate;

    [Header("Body")]
    [SerializeField] private GameObject body;

    [SerializeField] private SpecificPatchUI[] specificPatches;
    [SerializeField] private GameObject specificPatch;
    [SerializeField] private GameObject specificPatchParent;

    [SerializeField] private PatchReasonUI[] patchReasons;
    [SerializeField] private GameObject patchReason;
    [SerializeField] private GameObject patchReasonParent;

    private PatchHistory history;

    private bool opened;

    private void Awake()
    {
        itemButton.onClick.AddListener(Toggle);
    }

    public void Initialize(PatchHistory history)
    {
        this.history = history;

        SetOpen(false);

        Refresh();
    }

    public void Refresh()
    {
        currentSeasonText.text = $"시즌 {history.Season}-{history.SubSeason}";

        seasonWinrate.text = $"승률 : {history.Winrate:F1}%";

        seasonPickrate.text = $"픽률 : {history.Pickrate:F1}%";

        //------------------------------------------------
        // Stat
        //------------------------------------------------

        int statIndex = 0;

        foreach (CharacterStatType stat in System.Enum.GetValues(typeof(CharacterStatType)))
        {
            if (history.TryGetStatPatch(stat, out float before, out float after))
            {
                if (before == after) continue;

                Instantiate(specificPatch, specificPatchParent.transform).GetComponent<SpecificPatchUI>().Initialize(stat, before, after);
                statIndex++;
            }
        }

        while (statIndex < specificPatches.Length)
        {
            specificPatches[statIndex].gameObject.SetActive(false);
            statIndex++;
        }

        //------------------------------------------------
        // Reason
        //------------------------------------------------

        int reasonIndex = 0;

        foreach (PatchReason reason in history.GetReasons())
        {
            Debug.Log(reason);
            Instantiate(patchReason, patchReasonParent.transform).GetComponent<PatchReasonUI>().Initialize(reason);
            reasonIndex++;
        }

        while (reasonIndex < patchReasons.Length)
        {
            patchReasons[reasonIndex].gameObject.SetActive(false);
            reasonIndex++;
        }
    }

    public void Toggle()
    {
        SetOpen(!opened);
    }

    public void SetOpen(bool value)
    {
        opened = value;

        arrow.text = value ? "▼" : "▶";

        body.SetActive(value);
    }
}
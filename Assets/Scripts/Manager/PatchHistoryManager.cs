using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PatchHistoryManager : MonoBehaviour
{
    public static PatchHistoryManager Instance { get; private set; }

    public PatchManager patchManager;

    private readonly List<PatchRecord> currentSeasonRecords = new();

    private readonly Dictionary<int, List<PatchHistory>> histories = new();

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        patchManager.OnPatchApplied += AddRecord;
    }

    private void OnDisable()
    {
        if (PatchManager.Instance != null)
            PatchManager.Instance.OnPatchApplied -= AddRecord;
    }

    private void AddRecord(PatchRecord record)
    {
        Debug.Log(string.Join(", ", record.Reasons));
        currentSeasonRecords.Add(record);
    }

    public void SaveCurrentSeason()
    {
        Debug.Log($"SaveCurrentSeason : {currentSeasonRecords.Count}");

        foreach (RuntimeCharacter character in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            List<PatchRecord> records =
                currentSeasonRecords
                .Where(x => x.Character == character)
                .ToList();

            if (records.Count == 0)
                continue;

            PatchHistory history =
                new PatchHistory(
                    character,
                    SeasonManager.Instance.DisplaySeason,
                    SeasonManager.Instance.DisplaySubSeason,
                    AnalysisManager.Instance.GetAnalysis(character, AnalysisItem.Winrate).CurrentValue,
                    AnalysisManager.Instance.GetPickRate(character),
                    records);

            int id = character.OriginCharacter.id;

            if (!histories.ContainsKey(id))
                histories.Add(id, new List<PatchHistory>());

            histories[id].Add(history);
        }

        currentSeasonRecords.Clear();
    }

    public IReadOnlyList<PatchHistory> GetHistory(RuntimeCharacter character)
    {
        int id = character.OriginCharacter.id;

        if (histories.TryGetValue(id, out List<PatchHistory> result))
            return result;

        return System.Array.Empty<PatchHistory>();
    }

    public IReadOnlyList<PatchHistory> GetHistories(RuntimeCharacter character)
    {
        if (histories.TryGetValue(character.OriginCharacter.id, out var list))
            return list;

        return Array.Empty<PatchHistory>();
    }

    public void Clear()
    {
        histories.Clear();
        currentSeasonRecords.Clear();
    }
}
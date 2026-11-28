using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PatchAlertContentUI : MonoBehaviour
{
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text patchCount;

    private readonly HashSet<PatchRecord> patchRecords = new();

    public void Initialize(string name, PatchRecord record)
    {
        characterName.text = name;
        AddPatchRecord(record);
    }

    public void AddPatchRecord(PatchRecord record)
    {
        if (record == null || !patchRecords.Add(record))
            return;

        RefreshPatchCount();
    }

    private void RefreshPatchCount()
    {
        Dictionary<CharacterStatType, (float before, float after)> statChanges = new();

        foreach (PatchRecord record in patchRecords)
        {
            foreach (CharacterStatType stat in Enum.GetValues(typeof(CharacterStatType)))
            {
                if (!record.Patches.Any(patch => patch.StatType == stat))
                    continue;

                if (!statChanges.ContainsKey(stat))
                {
                    statChanges[stat] = (record.Before.Stats[stat].CurrentValue, record.After.Stats[stat].CurrentValue);
                }
                else
                {
                    var change = statChanges[stat];
                    statChanges[stat] = (change.before, record.After.Stats[stat].CurrentValue);
                }
            }
        }

        int count = 0;

        foreach (var change in statChanges.Values)
        {
            if (!Mathf.Approximately(change.before, change.after))
                count++;
        }

        patchCount.text = $" : {count} 개";
    }
}
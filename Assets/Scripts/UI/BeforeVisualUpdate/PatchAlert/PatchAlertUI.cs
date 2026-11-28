using System.Collections.Generic;
using UnityEngine;

public class PatchAlertUI : MonoBehaviour
{
    [SerializeField] private Transform AlertParent;
    [SerializeField] private GameObject AlertPrefab;

    private Dictionary<int, PatchAlertContentUI> alertDictionary = new();

    public void RegisterPatch(PatchRecord record)
    {
        int id = record.Character.OriginCharacter.id;

        if (alertDictionary.TryGetValue(id, out PatchAlertContentUI content))
        {
            content.Initialize(record.Character.OriginCharacter.characterName, record);
            return;
        }

        content = Instantiate(AlertPrefab, AlertParent).GetComponent<PatchAlertContentUI>();
        content.Initialize(record.Character.OriginCharacter.characterName, record);
        alertDictionary.Add(id, content);
    }

    public void ClearAlerts()
    {
        foreach (PatchAlertContentUI content in alertDictionary.Values)
        {
            if (content != null)
                Destroy(content.gameObject);
        }

        alertDictionary.Clear();
    }
}


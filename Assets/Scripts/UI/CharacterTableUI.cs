using System.Collections.Generic;
using UnityEngine;

public class CharacterTableUI : MonoBehaviour
{
    [SerializeField] private CharacterRowUI rowPrefab;
    [SerializeField] private Transform content;

    private readonly List<CharacterRowUI> rowList = new();
    [SerializeField] private List<GameObject> rankNumList;

    private void Start()
    {
        GenerateTable();
    }

    public void GenerateTable()
    {
        ClearTable();

        foreach (RuntimeCharacter runtimeCharacter in RuntimeCharacterManager.Instance.GetAllCharacters())
        {
            CharacterRowUI row = Instantiate(rowPrefab, content);

            row.Initialize(runtimeCharacter);

            rowList.Add(row);
        }

        DisplayRankNumber(rowList.Count);
    }

    public void RefreshTable()
    {
        foreach (CharacterRowUI row in rowList)
        {
            row.Refresh();
        }

        DisplayRankNumber(rowList.Count);
    }

    public void AddCharacter(RuntimeCharacter runtimeCharacter)
    {
        CharacterRowUI row = Instantiate(rowPrefab, content);

        row.Initialize(runtimeCharacter);

        rowList.Add(row);
    }

    private void ClearTable()
    {
        foreach (CharacterRowUI row in rowList)
        {
            if (row != null)
            {
                Destroy(row.gameObject);
            }
        }

        rowList.Clear();
    }

    private void DisplayRankNumber(int count)
    {
        for (int i = 0; i < 10; i++)
        {
            rankNumList[i].SetActive(count - 1 >= i);
        }
    }
}
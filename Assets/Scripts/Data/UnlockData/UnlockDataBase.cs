using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/UnlockData/Database")]
public class UnlockDataBase : ScriptableObject
{
    public List<UnlockData> unlocks;
}
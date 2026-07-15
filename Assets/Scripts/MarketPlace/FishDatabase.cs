using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishDatabase : MonoBehaviour
{
    public List<FishDataSO> allFish = new List<FishDataSO>();

    public FishDataSO GetRandomFish()
    {
        if (allFish == null || allFish.Count == 0)
        {
            Debug.LogWarning("FishDatabase is empty.");
            return null;
        }

        int index = Random.Range(0, allFish.Count);
        return allFish[index];
    }
}

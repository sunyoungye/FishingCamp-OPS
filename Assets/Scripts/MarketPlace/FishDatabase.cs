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

    public FishDataSO GetFishById(string fishId)
    {
        if (string.IsNullOrWhiteSpace(fishId))
        {
            return null;
        }

        foreach (FishDataSO fish in allFish)
        {
            if (fish != null && fish.fishId == fishId)
            {
                return fish;
            }
        }

        Debug.LogWarning($"Fish not found: {fishId}");
        return null;
    }
}

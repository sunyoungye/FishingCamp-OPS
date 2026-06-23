using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private Dictionary<string, int> fishInventory = new Dictionary<string, int>();

    public void AddFish(FishDataSO fish)
    {
        if (fish == null) return;

        if (fishInventory.ContainsKey(fish.fishId))
        {
            fishInventory[fish.fishId]++;
        }
        else
        {
            fishInventory.Add(fish.fishId, 1);
        }

        Debug.Log($"Inventory Added: {fish.fishName} x{fishInventory[fish.fishId]}");
    }

    public int GetFishCount(string fishId)
    {
        if (fishInventory.ContainsKey(fishId))
        {
            return fishInventory[fishId];
        }

        return 0;
    }
}

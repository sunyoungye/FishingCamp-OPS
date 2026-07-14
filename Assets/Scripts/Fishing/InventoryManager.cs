using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FishInventoryEntry
{
    public FishDataSO fish;
    public int count;

    public FishInventoryEntry(FishDataSO fish, int count)
    {
        this.fish = fish;
        this.count = count;
    }
}

public class InventoryManager : MonoBehaviour
{
    [Header("Capacity")]
    [SerializeField] private int maxCapacity = 60;

    [Header("Currency")]
    [SerializeField] private int coins = 0;

    private Dictionary<string, FishInventoryEntry> fishDictionary = new Dictionary<string, FishInventoryEntry>();

    public event Action OnInventoryChanged;
    public event Action<int> OnCoinsChanged;

    public int MaxCapacity => maxCapacity;
    public int Coins => coins;

    public int CapacityUsed
    {
        get
        {
            int total = 0;

            foreach (FishInventoryEntry entry in fishDictionary.Values)
            {
                total += entry.count;
            }

            return total;
        }
    }

    public bool AddFish(FishDataSO fish)
    {
        if (fish == null)
        {
            Debug.LogWarning("Tried to add null fish.");
            return false;
        }

        if (CapacityUsed >= maxCapacity)
        {
            Debug.LogWarning("Inventory is full.");
            return false;
        }

        if (fishDictionary.ContainsKey(fish.fishId))
        {
            fishDictionary[fish.fishId].count++;
        }
        else
        {
            fishDictionary.Add(fish.fishId, new FishInventoryEntry(fish, 1));
        }

        AddCoins(fish.coinReward);

        OnInventoryChanged?.Invoke();

        Debug.Log($"Inventory Added: {fish.fishName} x{fishDictionary[fish.fishId].count}");

        return true;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins);

        Debug.Log($"Coins: {coins}");
    }

    public List<FishInventoryEntry> GetAllFish()
    {
        return new List<FishInventoryEntry>(fishDictionary.Values);
    }

    public int GetFishCount(string fishId)
    {
        if (fishDictionary.ContainsKey(fishId))
        {
            return fishDictionary[fishId].count;
        }

        return 0;
    }
}
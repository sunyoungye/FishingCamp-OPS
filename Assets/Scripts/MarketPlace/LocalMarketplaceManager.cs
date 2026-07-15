using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalMarketplaceManager : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventoryManager;
    public CurrencyManager currencyManager;
    public FishDatabase fishDatabase;

    [Header("Random Market Settings")]
    public int initialListingCount = 4;
    public int maxRandomListings = 8;

    [Tooltip("For test: 5 seconds, in game: 180 seconds")]
    public float minSpawnInterval = 5f;

    public float maxSpawnInterval = 10f;

    public float minListingDuration = 30f;

    public float maxListingDuration = 60f;

    [Header("Price Settings")]
    public int minimumPrice = 30;
    public int maximumPrice = 800;

    private readonly List<MarketListing> randomListings = new List<MarketListing>();

    public event Action<List<MarketListing>> OnListingsChanged;

    private string[] sellerNames =
    {
        "SeaCat",
        "Erica",
        "Jiho",
        "Your Neighbor",
        "Jessica",
        "Hannah",
        "Dana",
        "John",
        "Maeve",
        "WaveFox",
        "Blair",
        "Messy"
    };

    private void Start()
    {
        CreateInitialListings();
        StartCoroutine(RandomListingRoutine());
    }

    private void Update()
    {
        UpdateListingTimers();
    }

    private void CreateInitialListings()
    {
        for (int i = 0; i < initialListingCount; i++)
        {
            TryCreateRandomListing();
        }
    }

    private IEnumerator RandomListingRoutine()
    {
        while (true)
        {
            float waitTime = UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            TryCreateRandomListing();
        }
    }

    public void TryCreateRandomListing()
    {
        if (randomListings.Count >= maxRandomListings)
        {
            return;
        }

        if (fishDatabase == null)
        {
            Debug.LogWarning("FishDatabase is not assigned.");
            return;
        }

        FishDataSO fish = fishDatabase.GetRandomFish();

        if (fish == null)
        {
            return;
        }

        MarketListing listing = new MarketListing
        {
            listingId = Guid.NewGuid().ToString(),
            fish = fish,
            quantity = 1,
            price = GetRandomPrice(fish),
            sellerName = sellerNames[UnityEngine.Random.Range(0, sellerNames.Length)],
            remainingTime = UnityEngine.Random.Range(minListingDuration, maxListingDuration),
            isSold = false,
            isPlayerListing = false
        };

        randomListings.Add(listing);

        Debug.Log($"Market Listing Created: {fish.fishName} / {listing.price} Coin");

        NotifyRandomListingsChanged();
    }

    public bool BuyListing(MarketListing listing)
    {
        if (listing == null || listing.fish == null)
        {
            Debug.LogWarning("Invalid listing.");
            return false;
        }

        if (listing.isSold)
        {
            Debug.LogWarning("This listing is already sold.");
            return false;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager is not assigned.");
            return false;
        }

        if (currencyManager == null)
        {
            Debug.LogWarning("CurrencyManager is not assigned.");
            return false;
        }

        if (inventoryManager.CapacityUsed >= inventoryManager.MaxCapacity)
        {
            Debug.LogWarning("Inventory is full.");
            return false;
        }

        bool paid = currencyManager.SpendCoins(listing.price);

        if (!paid)
        {
            Debug.LogWarning("Not enough coins.");
            return false;
        }

        bool added = inventoryManager.AddFish(listing.fish);

        if (!added)
        {
            Debug.LogWarning("Failed to add fish to inventory.");
            return false;
        }

        listing.isSold = true;
        randomListings.Remove(listing);

        Debug.Log($"Bought: {listing.fish.fishName} / {listing.price} Coin");

        NotifyRandomListingsChanged();

        return true;
    }

    public List<MarketListing> GetRandomListings()
    {
        return new List<MarketListing>(randomListings);
    }

    private void UpdateListingTimers()
    {
        bool changed = false;

        for (int i = randomListings.Count - 1; i >= 0; i--)
        {
            randomListings[i].remainingTime -= Time.deltaTime;

            if (randomListings[i].remainingTime <= 0f)
            {
                Debug.Log($"Market Listing Expired: {randomListings[i].fish.fishName}");
                randomListings.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
        {
            NotifyRandomListingsChanged();
        }
    }

    private int GetRandomPrice(FishDataSO fish)
    {
        int baseReward = 50;

        if (fish != null && fish.coinReward > 0)
        {
            baseReward = fish.coinReward;
        }

        float multiplier = UnityEngine.Random.Range(2.0f, 6.0f);
        int price = Mathf.RoundToInt(baseReward * multiplier);

        price = Mathf.Clamp(price, minimumPrice, maximumPrice);

        return price;
    }

    private void NotifyRandomListingsChanged()
    {
        OnListingsChanged?.Invoke(GetRandomListings());
    }
}

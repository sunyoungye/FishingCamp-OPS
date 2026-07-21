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
    public PlayerSessionManager playerSessionManager;

    [Header("Random NPC Market Settings")]
    public int initialListingCount = 4;

    [Tooltip("NPC 판매글 + 플레이어 판매글을 합친 전체 최대 개수")]
    public int maxRandomListings = 12;

    [Tooltip("For test: 5 seconds, in game: 180 seconds")]
    public float minSpawnInterval = 5f;

    [Tooltip("For test: 10 seconds, in game: 300 seconds")]
    public float maxSpawnInterval = 10f;

    [Tooltip("For test: 30 seconds, in game: 600 seconds")]
    public float minListingDuration = 30f;

    [Tooltip("For test: 60 seconds, in game: 900 seconds")]
    public float maxListingDuration = 60f;

    [Header("Player Listing Settings")]
    [Tooltip("내가 My Stall에서 올린 판매글 유지 시간")]
    public float playerListingDuration = 180f;

    [Header("Price Settings")]
    public int minimumPrice = 30;
    public int maximumPrice = 800;

    private readonly List<MarketListing> randomListings = new List<MarketListing>();

    public event Action<List<MarketListing>> OnListingsChanged;

    public event Action<List<MarketListing>> OnPlayerListingsChanged;

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
        NotifyRandomListingsChanged();
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
            fishId = fish.fishId,

            quantity = 1,
            price = GetRandomPrice(fish),

            sellerPlayerId = "NPC",
            sellerName = sellerNames[UnityEngine.Random.Range(0, sellerNames.Length)],

            remainingTime = UnityEngine.Random.Range(minListingDuration, maxListingDuration),
            isSold = false,

            isNpcListing = true
        };

        randomListings.Add(listing);
        NotifyRandomListingsChanged();

        Debug.Log($"NPC Market Listing Created: {fish.fishName} / {listing.price} Coin / by {listing.sellerName}");
    }

    public bool CreatePlayerListing(FishInventoryEntry entry, int price)
    {
        if (entry == null || entry.fish == null)
        {
            Debug.LogWarning("No fish selected.");
            return false;
        }

        if (price <= 0)
        {
            Debug.LogWarning("Invalid price.");
            return false;
        }

        if (randomListings.Count >= maxRandomListings)
        {
            Debug.LogWarning("Server Market is full.");
            return false;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager is not assigned.");
            return false;
        }

        string localPlayerId = GetLocalPlayerId();
        string localPlayerName = GetLocalPlayerName();

        bool removed = inventoryManager.RemoveFish(entry.fish.fishId, 1);

        if (!removed)
        {
            Debug.LogWarning("Failed to remove fish from inventory.");
            return false;
        }

        MarketListing listing = new MarketListing
        {
            listingId = Guid.NewGuid().ToString(),

            fish = entry.fish,
            fishId = entry.fish.fishId,

            quantity = 1,
            price = price,

            sellerPlayerId = localPlayerId,
            sellerName = localPlayerName,

            remainingTime = playerListingDuration,
            isSold = false,

            isNpcListing = false
        };

        randomListings.Add(listing);
        NotifyRandomListingsChanged();

        Debug.Log($"Player Listing Created: {entry.fish.fishName} / {price} Coin / by {localPlayerName}");

        return true;
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

        if (!randomListings.Contains(listing))
        {
            Debug.LogWarning("This listing no longer exists.");
            return false;
        }

        if (IsMyListing(listing))
        {
            Debug.LogWarning("You cannot buy your own listing.");
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
            currencyManager.AddCoins(listing.price);
            Debug.LogWarning("Failed to add fish to inventory. Coin refunded.");
            return false;
        }

        listing.isSold = true;
        randomListings.Remove(listing);

        if (listing.isNpcListing)
        {
            Debug.Log($"Bought NPC Listing: {listing.fish.fishName} / {listing.price} Coin");
        }
        else
        {
            Debug.Log($"Bought Player Listing: {listing.fish.fishName} / Seller payment will be handled later by network/server.");
        }

        NotifyRandomListingsChanged();

        return true;
    }

    public bool CancelPlayerListing(MarketListing listing)
    {
        if (listing == null || listing.fish == null)
        {
            Debug.LogWarning("Invalid listing.");
            return false;
        }

        if (!randomListings.Contains(listing))
        {
            Debug.LogWarning("Listing does not exist.");
            return false;
        }

        if (!IsMyListing(listing))
        {
            Debug.LogWarning("You can only cancel your own listing.");
            return false;
        }

        randomListings.Remove(listing);

        if (inventoryManager != null)
        {
            inventoryManager.AddFish(listing.fish);
        }

        NotifyRandomListingsChanged();

        Debug.Log($"Player Listing Cancelled: {listing.fish.fishName}");

        return true;
    }

    // 기존 코드 호환용
    public List<MarketListing> GetRandomListings()
    {
        return new List<MarketListing>(randomListings);
    }

    // 새 코드에서 Server Market 전체 목록을 받을 때 사용
    public List<MarketListing> GetListings()
    {
        return new List<MarketListing>(randomListings);
    }

    // My Stall에서 내가 올린 판매글만 받을 때 사용
    public List<MarketListing> GetMyListings()
    {
        List<MarketListing> myListings = new List<MarketListing>();

        string localPlayerId = GetLocalPlayerId();

        foreach (MarketListing listing in randomListings)
        {
            if (listing == null)
            {
                continue;
            }

            if (listing.isNpcListing)
            {
                continue;
            }

            if (listing.sellerPlayerId == localPlayerId)
            {
                myListings.Add(listing);
            }
        }

        return myListings;
    }

    public bool IsMyListing(MarketListing listing)
    {
        if (listing == null)
        {
            return false;
        }

        if (listing.isNpcListing)
        {
            return false;
        }

        return listing.sellerPlayerId == GetLocalPlayerId();
    }

    private void UpdateListingTimers()
    {
        bool changed = false;

        for (int i = randomListings.Count - 1; i >= 0; i--)
        {
            MarketListing listing = randomListings[i];

            if (listing == null)
            {
                randomListings.RemoveAt(i);
                changed = true;
                continue;
            }

            listing.remainingTime -= Time.deltaTime;

            if (listing.remainingTime <= 0f)
            {
                Debug.Log($"Market Listing Expired: {listing.fish.fishName}");

                if (!listing.isNpcListing && IsMyListing(listing))
                {
                    if (inventoryManager != null && listing.fish != null)
                    {
                        inventoryManager.AddFish(listing.fish);
                    }
                }

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
        if (fish == null)
        {
            return minimumPrice;
        }

        int baseReward = Mathf.Max(10, fish.coinReward);
        float multiplier = UnityEngine.Random.Range(2.0f, 6.0f);

        int price = Mathf.RoundToInt(baseReward * multiplier);
        price = Mathf.Clamp(price, minimumPrice, maximumPrice);

        return price;
    }

    private void NotifyRandomListingsChanged()
    {
        OnListingsChanged?.Invoke(GetRandomListings());
        OnPlayerListingsChanged?.Invoke(GetMyListings());
    }

    private string GetLocalPlayerId()
    {
        if (playerSessionManager != null && !string.IsNullOrEmpty(playerSessionManager.playerId))
        {
            return playerSessionManager.playerId;
        }

        return "LOCAL_PLAYER";
    }

    private string GetLocalPlayerName()
    {
        if (playerSessionManager != null && !string.IsNullOrEmpty(playerSessionManager.playerName))
        {
            return playerSessionManager.playerName;
        }

        return "Me";
    }
}
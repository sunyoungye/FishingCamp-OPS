using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class NetworkMarketplaceManager : NetworkBehaviour
{
    [Header("References")]
    public InventoryManager inventoryManager;
    public CurrencyManager currencyManager;
    public FishDatabase fishDatabase;

    [Header("Random NPC Market Settings")]
    public int initialListingCount = 4;
    public int maxListings = 12;

    public float minSpawnInterval = 5f;
    public float maxSpawnInterval = 10f;

    public float minListingDuration = 30f;
    public float maxListingDuration = 60f;

    [Header("Player Listing Settings")]
    public float playerListingDuration = 180f;

    [Header("Price Settings")]
    public int minimumPrice = 30;
    public int maximumPrice = 800;

    private readonly List<MarketListing> roomListings = new List<MarketListing>();

    public event Action<List<MarketListing>> OnListingsChanged;
    public event Action<List<MarketListing>> OnPlayerListingsChanged;

    private string[] npcSellerNames =
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
        "WaveFox"
    };

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CreateInitialNpcListings();
            StartCoroutine(RandomListingRoutine());

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }

            BroadcastListings();
            Debug.Log("NetworkMarketplaceManager spawned as Server/Host.");
        }
        else
        {
            Debug.Log("NetworkMarketplaceManager spawned as Client.");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void Update()
    {
        UpdateLocalTimerVisual();

        if (IsServer)
        {
            UpdateListingExpirationServer();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer)
        {
            return;
        }

        StartCoroutine(DelayedBroadcastListings());
    }

    private IEnumerator DelayedBroadcastListings()
    {
        yield return new WaitForSeconds(0.5f);
        BroadcastListings();
    }

    private void CreateInitialNpcListings()
    {
        for (int i = 0; i < initialListingCount; i++)
        {
            TryCreateNpcListingServerOnly(false);
        }
    }

    private IEnumerator RandomListingRoutine()
    {
        while (true)
        {
            float waitTime = UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            TryCreateNpcListingServerOnly(true);
        }
    }

    private void TryCreateNpcListingServerOnly(bool notify)
    {
        if (!IsServer)
        {
            return;
        }

        if (roomListings.Count >= maxListings)
        {
            return;
        }

        if (fishDatabase == null)
        {
            Debug.LogWarning("NetworkMarketplaceManager: FishDatabase is not assigned.");
            return;
        }

        FishDataSO fish = fishDatabase.GetRandomFish();

        if (fish == null)
        {
            return;
        }

        MarketListing listing = new MarketListing
        {
            listingId = Guid.NewGuid().ToString("N").Substring(0, 12),

            fish = fish,
            fishId = fish.fishId,

            quantity = 1,
            price = GetRandomPrice(fish),

            sellerPlayerId = "NPC",
            sellerName = npcSellerNames[UnityEngine.Random.Range(0, npcSellerNames.Length)],

            remainingTime = UnityEngine.Random.Range(minListingDuration, maxListingDuration),
            isSold = false,
            isNpcListing = true
        };

        roomListings.Add(listing);

        Debug.Log($"NPC Listing Created On Host: {fish.fishName} / {listing.price}");

        if (notify)
        {
            BroadcastListings();
        }
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

        if (roomListings.Count >= maxListings)
        {
            Debug.LogWarning("Server Market is full.");
            return false;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager is not assigned.");
            return false;
        }

        bool removed = inventoryManager.RemoveFish(entry.fish.fishId, 1);

        if (!removed)
        {
            Debug.LogWarning("Failed to remove fish from local inventory.");
            return false;
        }

        FixedString64Bytes fishId = entry.fish.fishId;
        FixedString64Bytes sellerName = GetLocalPlayerName();

        RequestCreatePlayerListingServerRpc(fishId, price, sellerName);

        Debug.Log($"Create Player Listing Requested: {entry.fish.fishName} / {price}");

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCreatePlayerListingServerRpc(
        FixedString64Bytes fishId,
        int price,
        FixedString64Bytes sellerName,
        ServerRpcParams rpcParams = default)
    {
        if (!IsServer)
        {
            return;
        }

        if (price <= 0)
        {
            return;
        }

        if (roomListings.Count >= maxListings)
        {
            return;
        }

        if (fishDatabase == null)
        {
            Debug.LogWarning("FishDatabase is not assigned on Host.");
            return;
        }

        FishDataSO fish = fishDatabase.GetFishById(fishId.ToString());

        if (fish == null)
        {
            Debug.LogWarning($"Fish not found on Host: {fishId}");
            return;
        }

        string sellerPlayerId = rpcParams.Receive.SenderClientId.ToString();

        MarketListing listing = new MarketListing
        {
            listingId = Guid.NewGuid().ToString("N").Substring(0, 12),

            fish = fish,
            fishId = fish.fishId,

            quantity = 1,
            price = price,

            sellerPlayerId = sellerPlayerId,
            sellerName = sellerName.ToString(),

            remainingTime = playerListingDuration,
            isSold = false,
            isNpcListing = false
        };

        roomListings.Add(listing);

        Debug.Log($"Player Listing Created On Host: {fish.fishName} / {price} / by {sellerName}");

        BroadcastListings();
    }

    public bool BuyListing(MarketListing listing)
    {
        if (listing == null || listing.fish == null)
        {
            return false;
        }

        if (IsMyListing(listing))
        {
            Debug.LogWarning("You cannot buy your own listing.");
            return false;
        }

        if (currencyManager != null)
        {
            bool paid = currencyManager.SpendCoins(listing.price);

            if (!paid)
            {
                Debug.LogWarning("Not enough coins.");
                return false;
            }
        }

        if (inventoryManager != null)
        {
            bool added = inventoryManager.AddFish(listing.fish);

            if (!added)
            {
                Debug.LogWarning("Failed to add fish to inventory.");
                return false;
            }
        }

        FixedString64Bytes listingId = listing.listingId;
        RequestBuyListingServerRpc(listingId);

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestBuyListingServerRpc(
        FixedString64Bytes listingId,
        ServerRpcParams rpcParams = default)
    {
        if (!IsServer)
        {
            return;
        }

        for (int i = roomListings.Count - 1; i >= 0; i--)
        {
            if (roomListings[i].listingId == listingId.ToString())
            {
                Debug.Log($"Listing Bought On Host: {roomListings[i].fish.fishName}");
                roomListings.RemoveAt(i);
                BroadcastListings();
                return;
            }
        }
    }

    public bool CancelPlayerListing(MarketListing listing)
    {
        if (listing == null || listing.fish == null)
        {
            return false;
        }

        if (!IsMyListing(listing))
        {
            Debug.LogWarning("You can only cancel your own listing.");
            return false;
        }

        if (inventoryManager != null)
        {
            inventoryManager.AddFish(listing.fish);
        }

        FixedString64Bytes listingId = listing.listingId;
        RequestCancelListingServerRpc(listingId);

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCancelListingServerRpc(
        FixedString64Bytes listingId,
        ServerRpcParams rpcParams = default)
    {
        if (!IsServer)
        {
            return;
        }

        string requesterId = rpcParams.Receive.SenderClientId.ToString();

        for (int i = roomListings.Count - 1; i >= 0; i--)
        {
            MarketListing listing = roomListings[i];

            if (listing.listingId != listingId.ToString())
            {
                continue;
            }

            if (listing.sellerPlayerId != requesterId)
            {
                Debug.LogWarning("Cancel denied. Not owner.");
                return;
            }

            roomListings.RemoveAt(i);

            Debug.Log($"Listing Cancelled On Host: {listing.fish.fishName}");

            BroadcastListings();
            return;
        }
    }

    private void BroadcastListings()
    {
        if (!IsServer)
        {
            return;
        }

        RoomMarketListingNetData[] dataArray = new RoomMarketListingNetData[roomListings.Count];

        for (int i = 0; i < roomListings.Count; i++)
        {
            MarketListing listing = roomListings[i];

            RoomMarketListingNetData data = new RoomMarketListingNetData
            {
                listingId = listing.listingId,
                fishId = listing.fishId,

                quantity = listing.quantity,
                price = listing.price,

                sellerPlayerId = listing.sellerPlayerId,
                sellerName = listing.sellerName,

                remainingTime = listing.remainingTime,
                isSold = listing.isSold,
                isNpcListing = listing.isNpcListing
            };

            dataArray[i] = data;
        }

        SyncListingsClientRpc(dataArray);
    }

    [ClientRpc]
    private void SyncListingsClientRpc(RoomMarketListingNetData[] dataArray)
    {
        roomListings.Clear();

        foreach (RoomMarketListingNetData data in dataArray)
        {
            if (fishDatabase == null)
            {
                Debug.LogWarning("FishDatabase is not assigned on Client.");
                continue;
            }

            FishDataSO fish = fishDatabase.GetFishById(data.fishId.ToString());

            if (fish == null)
            {
                Debug.LogWarning($"Fish not found on Client: {data.fishId}");
                continue;
            }

            MarketListing listing = new MarketListing
            {
                listingId = data.listingId.ToString(),

                fish = fish,
                fishId = data.fishId.ToString(),

                quantity = data.quantity,
                price = data.price,

                sellerPlayerId = data.sellerPlayerId.ToString(),
                sellerName = data.sellerName.ToString(),

                remainingTime = data.remainingTime,
                isSold = data.isSold,
                isNpcListing = data.isNpcListing
            };

            roomListings.Add(listing);
        }

        NotifyListingsChanged();

        Debug.Log($"Marketplace Synced. Listings Count: {roomListings.Count}");
    }

    private void NotifyListingsChanged()
    {
        OnListingsChanged?.Invoke(GetRandomListings());
        OnPlayerListingsChanged?.Invoke(GetMyListings());
    }

    private void UpdateLocalTimerVisual()
    {
        for (int i = 0; i < roomListings.Count; i++)
        {
            if (roomListings[i] != null)
            {
                roomListings[i].remainingTime -= Time.deltaTime;
                roomListings[i].remainingTime = Mathf.Max(0f, roomListings[i].remainingTime);
            }
        }
    }

    private void UpdateListingExpirationServer()
    {
        bool changed = false;

        for (int i = roomListings.Count - 1; i >= 0; i--)
        {
            MarketListing listing = roomListings[i];

            if (listing == null)
            {
                roomListings.RemoveAt(i);
                changed = true;
                continue;
            }

            if (listing.remainingTime <= 0f)
            {
                Debug.Log($"Listing Expired On Host: {listing.fish.fishName}");
                roomListings.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
        {
            BroadcastListings();
        }
    }

    public List<MarketListing> GetRandomListings()
    {
        return new List<MarketListing>(roomListings);
    }

    public List<MarketListing> GetListings()
    {
        return new List<MarketListing>(roomListings);
    }

    public List<MarketListing> GetMyListings()
    {
        List<MarketListing> myListings = new List<MarketListing>();

        string localPlayerId = GetLocalPlayerId();

        foreach (MarketListing listing in roomListings)
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

    private string GetLocalPlayerId()
    {
        if (NetworkManager.Singleton == null)
        {
            return "LOCAL_PLAYER";
        }

        return NetworkManager.Singleton.LocalClientId.ToString();
    }

    private string GetLocalPlayerName()
    {
        return PlayerPrefs.GetString("PLAYER_NAME", "Player");
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
}
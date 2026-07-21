using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketPlace_PanelUI : MonoBehaviour
{
    private enum MarketTab
    {
        OceanMarket,
        MyStall
    }

    [Header("Managers")]
    public LocalMarketplaceManager marketplaceManager;
    public InventoryManager inventoryManager;

    [Header("Panel")]
    public GameObject panelRoot;
    public Button marketplaceButton;
    public Button closeButton;

    [Header("Tab Buttons")]
    public Button oceanMarketTabButton;
    public Button myStallTabButton;

    [Header("Views")]
    public GameObject oceanMarketView;
    public GameObject myStallView;

    [Header("Listing List")]
    public Transform oceanListingContent;
    public MarketListingCardUI listingCardPrefab;

    [Header("Selected Listing")]
    public GameObject selectedListingPanel;
    public Image selectedFishIcon;
    public TMP_Text selectedFishNameText;
    public TMP_Text selectedSellerText;
    public TMP_Text selectedPriceText;
    public TMP_Text selectedTimerText;
    public TMP_Text selectedDescriptionText;
    public Button buyNowButton;

    [Header("Create Sell Shortcut")]
    public Button createNewSellButton;

    [Header("My Stall Inventory")]
    public Transform myFishContent;
    public MyStallFishSlotUI myStallFishSlotPrefab;

    [Header("My Stall Sell Setup")]
    public Image selectedSellFishIcon;
    public TMP_Text selectedSellFishNameText;
    public TMP_Text selectedSellFishCountText;
    public TMP_InputField priceInputField;
    public Button priceMinusButton;
    public Button pricePlusButton;
    public Button registerSellButton;
    public TMP_Text recommendedPriceText;

    [Header("My Stall Listings")]
    public Transform myListingsContent;
    public MyStallListingCardUI myStallListingCardPrefab;
    public TMP_Text listingCountText;

    [Header("Optional Old Text")]
    public TMP_Text myStallInfoText;

    private MarketTab currentTab = MarketTab.OceanMarket;

    private MarketListing selectedListing;
    private MarketListingCardUI selectedCard;

    private FishInventoryEntry selectedSellEntry;
    private MyStallFishSlotUI selectedSellSlot;
    private int currentSellPrice = 0;

    private void Start()
    {
        if (marketplaceButton != null)
        {
            marketplaceButton.onClick.AddListener(OpenPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        if (oceanMarketTabButton != null)
        {
            oceanMarketTabButton.onClick.AddListener(() => SetTab(MarketTab.OceanMarket));
        }

        if (myStallTabButton != null)
        {
            myStallTabButton.onClick.AddListener(() => SetTab(MarketTab.MyStall));
        }

        if (buyNowButton != null)
        {
            buyNowButton.onClick.AddListener(OnBuyNowClicked);
        }

        if (createNewSellButton != null)
        {
            createNewSellButton.onClick.AddListener(() => SetTab(MarketTab.MyStall));
        }

        if (registerSellButton != null)
        {
            registerSellButton.onClick.AddListener(OnRegisterSellClicked);
        }

        if (priceMinusButton != null)
        {
            priceMinusButton.onClick.AddListener(DecreasePrice);
        }

        if (pricePlusButton != null)
        {
            pricePlusButton.onClick.AddListener(IncreasePrice);
        }

        if (marketplaceManager != null)
        {
            marketplaceManager.OnListingsChanged += RefreshOceanMarket;
            marketplaceManager.OnPlayerListingsChanged += RefreshMyListings;
        }
        else
        {
            Debug.LogWarning("MarketPlace_PanelUI: LocalMarketplaceManager is not assigned.");
        }

        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged += RefreshMyInventory;
        }
        else
        {
            Debug.LogWarning("MarketPlace_PanelUI: InventoryManager is not assigned.");
        }

        RefreshSellSetupPanel();
        ClosePanel();
    }

    private void Update()
    {
        UpdateSelectedTimerText();
    }

    private void OnDestroy()
    {
        if (marketplaceManager != null)
        {
            marketplaceManager.OnListingsChanged -= RefreshOceanMarket;
            marketplaceManager.OnPlayerListingsChanged -= RefreshMyListings;
        }

        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= RefreshMyInventory;
        }
    }

    public void OpenPanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        SetTab(currentTab);
    }

    public void ClosePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void SetTab(MarketTab tab)
    {
        currentTab = tab;

        if (oceanMarketView != null)
        {
            oceanMarketView.SetActive(tab == MarketTab.OceanMarket);
        }

        if (myStallView != null)
        {
            myStallView.SetActive(tab == MarketTab.MyStall);
        }

        if (tab == MarketTab.OceanMarket)
        {
            if (marketplaceManager != null)
            {
                RefreshOceanMarket(marketplaceManager.GetRandomListings());
            }
        }
        else
        {
            if (myStallInfoText != null)
            {
                myStallInfoText.gameObject.SetActive(false);
            }

            RefreshMyInventory();

            if (marketplaceManager != null)
            {
                RefreshMyListings(marketplaceManager.GetMyListings());
            }
        }
    }

    private void RefreshOceanMarket(List<MarketListing> listings)
    {
        if (oceanListingContent == null || listingCardPrefab == null)
        {
            Debug.LogWarning("RefreshOceanMarket failed: content or prefab is missing.");
            return;
        }

        if (listings == null)
        {
            listings = new List<MarketListing>();
        }

        bool selectedStillExists = false;

        if (selectedListing != null)
        {
            foreach (MarketListing listing in listings)
            {
                if (listing != null && listing.listingId == selectedListing.listingId)
                {
                    selectedStillExists = true;
                    selectedListing = listing;
                    break;
                }
            }
        }

        if (!selectedStillExists)
        {
            selectedListing = null;
            selectedCard = null;
        }

        foreach (Transform child in oceanListingContent)
        {
            Destroy(child.gameObject);
        }

        selectedCard = null;

        MarketListing firstListing = null;
        MarketListingCardUI firstCard = null;

        foreach (MarketListing listing in listings)
        {
            if (listing == null || listing.fish == null)
            {
                continue;
            }

            MarketListingCardUI card = Instantiate(listingCardPrefab, oceanListingContent);
            card.Setup(listing, OnListingCardClicked);

            if (firstListing == null)
            {
                firstListing = listing;
                firstCard = card;
            }

            if (selectedListing != null && listing.listingId == selectedListing.listingId)
            {
                selectedCard = card;
                selectedCard.SetSelected(true);
            }
        }

        if (selectedListing == null && firstListing != null)
        {
            OnListingCardClicked(firstListing, firstCard);
        }
        else
        {
            RefreshSelectedListingPanel();
        }
    }

    private void OnListingCardClicked(MarketListing listing, MarketListingCardUI card)
    {
        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
        }

        selectedListing = listing;
        selectedCard = card;

        if (selectedCard != null)
        {
            selectedCard.SetSelected(true);
        }

        RefreshSelectedListingPanel();
    }

    private void RefreshSelectedListingPanel()
    {
        bool hasSelected = selectedListing != null && selectedListing.fish != null;

        if (selectedListingPanel != null)
        {
            selectedListingPanel.SetActive(hasSelected);
        }

        if (!hasSelected)
        {
            return;
        }

        bool isMyListing = false;

        if (marketplaceManager != null)
        {
            isMyListing = marketplaceManager.IsMyListing(selectedListing);
        }

        if (selectedFishIcon != null)
        {
            selectedFishIcon.sprite = selectedListing.fish.fishSprite;
            selectedFishIcon.preserveAspect = true;
            selectedFishIcon.enabled = selectedListing.fish.fishSprite != null;
        }

        if (selectedFishNameText != null)
        {
            selectedFishNameText.text = selectedListing.fish.fishName;
        }

        if (selectedSellerText != null)
        {
            selectedSellerText.text = $"by {selectedListing.sellerName}";
        }

        if (selectedPriceText != null)
        {
            selectedPriceText.text = selectedListing.price.ToString();
        }

        if (selectedDescriptionText != null)
        {
            if (isMyListing)
            {
                selectedDescriptionText.text =
                    "내가 등록한 판매글입니다.\n" +
                    "My Stall에서 취소할 수 있어요.";
            }
            else if (selectedListing.isNpcListing)
            {
                selectedDescriptionText.text =
                    $"Rarity: {selectedListing.fish.rarity}\n" +
                    "A fresh catch from the ocean market.";
            }
            else
            {
                selectedDescriptionText.text =
                    $"Rarity: {selectedListing.fish.rarity}\n" +
                    "A listing from another player.";
            }
        }

        if (buyNowButton != null)
        {
            buyNowButton.interactable = !isMyListing;
        }

        UpdateSelectedTimerText();
    }

    private void UpdateSelectedTimerText()
    {
        if (selectedListing == null || selectedTimerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(selectedListing.remainingTime));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        selectedTimerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void OnBuyNowClicked()
    {
        if (selectedListing == null || marketplaceManager == null)
        {
            return;
        }

        bool success = marketplaceManager.BuyListing(selectedListing);

        if (success)
        {
            selectedListing = null;
            selectedCard = null;
            RefreshOceanMarket(marketplaceManager.GetRandomListings());
        }
    }

    private void RefreshMyInventory()
    {
        if (myFishContent == null)
        {
            Debug.LogWarning("RefreshMyInventory failed: MyFishContent is not assigned.");
            return;
        }

        if (myStallFishSlotPrefab == null)
        {
            Debug.LogWarning("RefreshMyInventory failed: MyStallFishSlotPrefab is not assigned.");
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning("RefreshMyInventory failed: InventoryManager is not assigned.");
            return;
        }

        foreach (Transform child in myFishContent)
        {
            Destroy(child.gameObject);
        }

        List<FishInventoryEntry> fishList = inventoryManager.GetAllFish();

        Debug.Log($"My Stall Inventory Refresh: {fishList.Count} fish types found.");

        selectedSellSlot = null;

        bool selectedStillExists = false;

        if (selectedSellEntry != null && selectedSellEntry.fish != null)
        {
            foreach (FishInventoryEntry entry in fishList)
            {
                if (entry != null &&
                    entry.fish != null &&
                    entry.fish.fishId == selectedSellEntry.fish.fishId)
                {
                    selectedStillExists = true;
                    selectedSellEntry = entry;
                    break;
                }
            }
        }

        if (!selectedStillExists)
        {
            selectedSellEntry = null;
        }

        foreach (FishInventoryEntry entry in fishList)
        {
            if (entry == null || entry.fish == null)
            {
                continue;
            }

            MyStallFishSlotUI slot = Instantiate(myStallFishSlotPrefab, myFishContent);
            slot.Setup(entry, OnMyFishSlotClicked);

            if (selectedSellEntry != null &&
                selectedSellEntry.fish != null &&
                entry.fish.fishId == selectedSellEntry.fish.fishId)
            {
                selectedSellSlot = slot;
                selectedSellSlot.SetSelected(true);
            }
        }

        RefreshSellSetupPanel();
    }

    private void OnMyFishSlotClicked(FishInventoryEntry entry, MyStallFishSlotUI slot)
    {
        if (selectedSellSlot != null)
        {
            selectedSellSlot.SetSelected(false);
        }

        selectedSellEntry = entry;
        selectedSellSlot = slot;

        if (selectedSellSlot != null)
        {
            selectedSellSlot.SetSelected(true);
        }

        SetRecommendedPriceFromFish(entry);
        RefreshSellSetupPanel();
    }

    private void RefreshSellSetupPanel()
    {
        bool hasSelection = selectedSellEntry != null && selectedSellEntry.fish != null;

        if (selectedSellFishIcon != null)
        {
            selectedSellFishIcon.enabled = hasSelection;

            if (hasSelection)
            {
                selectedSellFishIcon.sprite = selectedSellEntry.fish.fishSprite;
                selectedSellFishIcon.preserveAspect = true;
            }
        }

        if (selectedSellFishNameText != null)
        {
            selectedSellFishNameText.text = hasSelection ? selectedSellEntry.fish.fishName : "Select Fish";
        }

        if (selectedSellFishCountText != null)
        {
            selectedSellFishCountText.text = hasSelection ? $"보유 수량  x{selectedSellEntry.count}" : "보유 수량  x0";
        }

        if (registerSellButton != null)
        {
            registerSellButton.interactable = hasSelection;
        }

        if (!hasSelection)
        {
            currentSellPrice = 0;

            if (priceInputField != null)
            {
                priceInputField.text = "";
            }

            if (recommendedPriceText != null)
            {
                recommendedPriceText.text = "권장 가격: -";
            }
        }
    }

    private void SetRecommendedPriceFromFish(FishInventoryEntry entry)
    {
        if (entry == null || entry.fish == null)
        {
            return;
        }

        int basePrice = Mathf.Max(10, entry.fish.coinReward);
        int minPrice = Mathf.RoundToInt(basePrice * 2f);
        int maxPrice = Mathf.RoundToInt(basePrice * 4f);

        currentSellPrice = Mathf.RoundToInt((minPrice + maxPrice) * 0.5f);

        if (priceInputField != null)
        {
            priceInputField.text = currentSellPrice.ToString();
        }

        if (recommendedPriceText != null)
        {
            recommendedPriceText.text = $"권장 가격: {minPrice} ~ {maxPrice}";
        }
    }

    private void DecreasePrice()
    {
        if (currentSellPrice <= 0)
        {
            return;
        }

        currentSellPrice = Mathf.Max(1, currentSellPrice - 10);

        if (priceInputField != null)
        {
            priceInputField.text = currentSellPrice.ToString();
        }
    }

    private void IncreasePrice()
    {
        if (currentSellPrice <= 0)
        {
            currentSellPrice = 10;
        }
        else
        {
            currentSellPrice += 10;
        }

        if (priceInputField != null)
        {
            priceInputField.text = currentSellPrice.ToString();
        }
    }

    private void OnRegisterSellClicked()
    {
        if (selectedSellEntry == null || selectedSellEntry.fish == null)
        {
            Debug.LogWarning("No fish selected for selling.");
            return;
        }

        if (priceInputField == null)
        {
            Debug.LogWarning("PriceInputField is not assigned.");
            return;
        }

        if (marketplaceManager == null)
        {
            Debug.LogWarning("MarketplaceManager is not assigned.");
            return;
        }

        int price;

        if (!int.TryParse(priceInputField.text, out price))
        {
            Debug.LogWarning("Invalid price input.");
            return;
        }

        if (price <= 0)
        {
            Debug.LogWarning("Price must be greater than 0.");
            return;
        }

        bool success = marketplaceManager.CreatePlayerListing(selectedSellEntry, price);

        if (success)
        {
            Debug.Log($"Registered listing: {selectedSellEntry.fish.fishName} / {price}");

            priceInputField.text = "";
            selectedSellEntry = null;
            selectedSellSlot = null;
            currentSellPrice = 0;

            RefreshMyInventory();
            RefreshSellSetupPanel();

            RefreshMyListings(marketplaceManager.GetMyListings());
            RefreshOceanMarket(marketplaceManager.GetRandomListings());
        }
    }

    private void RefreshMyListings(List<MarketListing> listings)
    {
        if (myListingsContent == null || myStallListingCardPrefab == null)
        {
            return;
        }

        if (listings == null)
        {
            listings = new List<MarketListing>();
        }

        foreach (Transform child in myListingsContent)
        {
            Destroy(child.gameObject);
        }

        foreach (MarketListing listing in listings)
        {
            if (listing == null || listing.fish == null)
            {
                continue;
            }

            MyStallListingCardUI card = Instantiate(myStallListingCardPrefab, myListingsContent);
            card.Setup(listing, OnCancelPlayerListingClicked);
        }

        UpdateListingCountText(listings.Count);
    }

    private void OnCancelPlayerListingClicked(MarketListing listing)
    {
        if (marketplaceManager == null)
        {
            return;
        }

        bool success = marketplaceManager.CancelPlayerListing(listing);

        if (success)
        {
            RefreshMyInventory();
            RefreshMyListings(marketplaceManager.GetMyListings());
            RefreshOceanMarket(marketplaceManager.GetRandomListings());
        }
    }

    private void UpdateListingCountText(int count)
    {
        if (listingCountText != null)
        {
            listingCountText.text = $"등록 가능 {count}/10";
        }
    }
}
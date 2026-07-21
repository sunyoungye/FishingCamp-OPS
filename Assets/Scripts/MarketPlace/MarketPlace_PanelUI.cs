using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    [Header("Create Sell")]
    public Button createNewSellButton;

    [Header("My Stall")]
    public TMP_Text myStallInfoText;

    private MarketTab currentTab = MarketTab.OceanMarket;
    private MarketListing selectedListing;
    private MarketListingCardUI selectedCard;

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

        if (marketplaceManager != null)
        {
            marketplaceManager.OnListingsChanged += RefreshOceanMarket;
        }

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
                myStallInfoText.text = "My Stall is next step";
            }
        }
    }

    private void RefreshOceanMarket(List<MarketListing> listings)
    {
        if (oceanListingContent == null || listingCardPrefab == null)
        {
            return;
        }

        bool selectedStillExists = false;

        if (selectedListing != null)
        {
            foreach (MarketListing listing in listings)
            {
                if (listing.listingId == selectedListing.listingId)
                {
                    selectedStillExists = true;
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
            selectedDescriptionText.text =
                $"Rarity: {selectedListing.fish.rarity}\n" +
                "A fresh catch from the ocean market.";
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
}

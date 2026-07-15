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

    [Header("Ocean Market")]
    public Transform oceanListingContent;
    public MarketListingCardUI listingCardPrefab;

    [Header("My Stall")]
    public TMP_Text myStallInfoText;

    private MarketTab currentTab = MarketTab.OceanMarket;

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

        if (marketplaceManager != null)
        {
            marketplaceManager.OnListingsChanged += RefreshOceanMarket;
        }

        ClosePanel();
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

        foreach (Transform child in oceanListingContent)
        {
            Destroy(child.gameObject);
        }

        foreach (MarketListing listing in listings)
        {
            MarketListingCardUI card = Instantiate(listingCardPrefab, oceanListingContent);
            card.Setup(listing, OnBuyClicked);
        }
    }

    private void OnBuyClicked(MarketListing listing)
    {
        if (marketplaceManager != null)
        {
            marketplaceManager.BuyListing(listing);
        }
    }
}

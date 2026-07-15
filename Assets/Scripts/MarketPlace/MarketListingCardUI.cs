using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketListingCardUI : MonoBehaviour
{
    [Header("UI")]
    public Image fishIcon;
    public TMP_Text fishNameText;
    public TMP_Text rarityText;
    public TMP_Text sellerNameText;
    public TMP_Text priceText;
    public TMP_Text timerText;
    public Button buyButton;

    private MarketListing currentListing;
    private Action<MarketListing> onBuyClicked;

    public void Setup(MarketListing listing, Action<MarketListing> buyCallback)
    {
        currentListing = listing;
        onBuyClicked = buyCallback;

        if (listing == null || listing.fish == null)
        {
            return;
        }

        if (fishIcon != null)
        {
            fishIcon.sprite = listing.fish.fishSprite;
            fishIcon.preserveAspect = true;
        }

        if (fishNameText != null)
        {
            fishNameText.text = listing.fish.fishName;
        }

        if (rarityText != null)
        {
            rarityText.text = listing.fish.rarity.ToString();
        }

        if (sellerNameText != null)
        {
            sellerNameText.text = $"Seller: {listing.sellerName}";
        }

        if (priceText != null)
        {
            priceText.text = $"{listing.price} Coin";
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        UpdateTimerText();
    }

    private void Update()
    {
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (currentListing == null || timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(currentListing.remainingTime));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void OnBuyButtonClicked()
    {
        onBuyClicked?.Invoke(currentListing);
    }
}

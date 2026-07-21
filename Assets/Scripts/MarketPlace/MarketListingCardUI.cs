using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketListingCardUI : MonoBehaviour
{
    [Header("UI")]
    public Button cardButton;
    public GameObject selectedFrame;

    public Image fishIcon;
    public TMP_Text fishNameText;
    public TMP_Text sellerNameText;
    public TMP_Text priceText;
    public TMP_Text timerText;

    private MarketListing currentListing;
    private Action<MarketListing, MarketListingCardUI> onCardClicked;

    public void Setup(MarketListing listing, Action<MarketListing, MarketListingCardUI> clickCallback)
    {
        currentListing = listing;
        onCardClicked = clickCallback;

        if (listing == null || listing.fish == null)
        {
            return;
        }

        if (fishIcon != null)
        {
            fishIcon.sprite = listing.fish.fishSprite;
            fishIcon.preserveAspect = true;
            fishIcon.enabled = listing.fish.fishSprite != null;
        }

        if (fishNameText != null)
        {
            fishNameText.text = listing.fish.fishName;
        }

        if (sellerNameText != null)
        {
            sellerNameText.text = $"by {listing.sellerName}";
        }

        if (priceText != null)
        {
            priceText.text = listing.price.ToString();
        }

        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnClickCard);
        }

        SetSelected(false);
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

    private void OnClickCard()
    {
        onCardClicked?.Invoke(currentListing, this);
    }

    public void SetSelected(bool selected)
    {
        if (selectedFrame != null)
        {
            selectedFrame.SetActive(selected);
        }
    }
}

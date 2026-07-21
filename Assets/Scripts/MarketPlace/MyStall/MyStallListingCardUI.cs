using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyStallListingCardUI : MonoBehaviour
{
    public Image fishIcon;
    public TMP_Text fishNameText;
    public TMP_Text priceText;
    public TMP_Text registeredTimeText;
    public TMP_Text remainingTimeText;
    public Button cancelButton;

    private MarketListing currentListing;
    private Action<MarketListing> onCancelClicked;

    public void Setup(MarketListing listing, Action<MarketListing> cancelCallback)
    {
        currentListing = listing;
        onCancelClicked = cancelCallback;

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

        if (priceText != null)
        {
            priceText.text = listing.price.ToString();
        }

        if (registeredTimeText != null)
        {
            registeredTimeText.text = "등록 시간 13:10";
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancel);
        }

        UpdateRemainingTimeText();
    }

    private void Update()
    {
        UpdateRemainingTimeText();
    }

    private void UpdateRemainingTimeText()
    {
        if (currentListing == null || remainingTimeText == null)
        {
            return;
        }

        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(currentListing.remainingTime));
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        remainingTimeText.text = $"남은 시간 {hours:00}:{minutes:00}:{seconds:00}";
    }

    private void OnCancel()
    {
        onCancelClicked?.Invoke(currentListing);
    }
}

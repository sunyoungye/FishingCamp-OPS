using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyStallFishSlotUI : MonoBehaviour
{
    public Button button;
    public GameObject selectedFrame;
    public GameObject selectedCheckIcon;

    public Image fishIcon;
    public TMP_Text fishNameText;
    public TMP_Text countText;

    private FishInventoryEntry currentEntry;
    private Action<FishInventoryEntry, MyStallFishSlotUI> onClicked;

    public void Setup(FishInventoryEntry entry, Action<FishInventoryEntry, MyStallFishSlotUI> clickCallback)
    {
        currentEntry = entry;
        onClicked = clickCallback;

        if (entry == null || entry.fish == null)
        {
            return;
        }

        if (fishIcon != null)
        {
            fishIcon.sprite = entry.fish.fishSprite;
            fishIcon.preserveAspect = true;
            fishIcon.enabled = entry.fish.fishSprite != null;
        }

        if (fishNameText != null)
        {
            fishNameText.text = entry.fish.fishName;
        }

        if (countText != null)
        {
            countText.text = $"º¸À¯ x{entry.count}";
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        SetSelected(false);
    }

    private void OnClick()
    {
        onClicked?.Invoke(currentEntry, this);
    }

    public void SetSelected(bool selected)
    {
        if (selectedFrame != null)
        {
            selectedFrame.SetActive(selected);
        }

        if (selectedCheckIcon != null)
        {
            selectedCheckIcon.SetActive(selected);
        }
    }
}
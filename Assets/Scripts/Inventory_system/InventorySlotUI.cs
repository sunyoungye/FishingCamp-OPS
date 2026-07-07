using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventorySlotUI : MonoBehaviour
{
    public Button button;
    public Image backgroundImage;
    public Image itemIcon;
    public TMP_Text countText;

    public Sprite normalSprite;
    public Sprite selectedSprite;

    private FishInventoryEntry currentEntry;
    private Action<FishInventoryEntry, InventorySlotUI> onClicked;

    public void Setup(FishInventoryEntry entry, Action<FishInventoryEntry, InventorySlotUI> clickCallback)
    {
        currentEntry = entry;
        onClicked = clickCallback;

        if (entry == null || entry.fish == null)
        {
            return;
        }

        if (itemIcon != null)
        {
            itemIcon.sprite = entry.fish.fishSprite;
            itemIcon.preserveAspect = true;
        }

        if (countText != null)
        {
            countText.text = $"x{entry.count}";
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickSlot);
        }

        SetSelected(false);
    }

    private void OnClickSlot()
    {
        onClicked?.Invoke(currentEntry, this);
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage == null) return;

        if (selected && selectedSprite != null)
        {
            backgroundImage.sprite = selectedSprite;
        }
        else if (!selected && normalSprite != null)
        {
            backgroundImage.sprite = normalSprite;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryPanelUI : MonoBehaviour
{
    private enum InventoryTab
    {
        Fish,
        Furniture,
        Material
    }

    [Header("Managers")]
    public InventorySystem inventorySystem;

    [Header("Panel Slide")]
    public RectTransform panelRoot;
    public CanvasGroup panelCanvasGroup;
    public Button inventoryButton;
    public Button closeButton;

    public Vector2 openPosition = new Vector2(0f, 0f);
    public Vector2 closedPosition = new Vector2(900f, 0f);
    public float slideDuration = 0.25f;

    [Header("Tabs")]
    public Button fishTabButton;
    public Button furnitureTabButton;
    public Button materialTabButton;

    [Header("Grid")]
    public Transform itemGridContent;
    public InventorySlotUI slotPrefab;

    [Header("Detail UI")]
    public Image detailIcon;
    public TMP_Text detailNameText;
    public TMP_Text detailInfoText;

    [Header("Capacity UI")]
    public TMP_Text capacityText;
    public Image capacityFillImage;

    public Color enoughColor = new Color(0.35f, 0.65f, 0.35f);
    public Color warningColor = new Color(0.95f, 0.75f, 0.15f);
    public Color fullColor = new Color(0.85f, 0.25f, 0.18f);

    private InventoryTab currentTab = InventoryTab.Fish;
    private bool isOpen = false;
    private Coroutine slideRoutine;
    private InventorySlotUI selectedSlot;

    private void Start()
    {
        if (inventoryButton != null)
        {
            inventoryButton.onClick.AddListener(TogglePanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        if (fishTabButton != null)
        {
            fishTabButton.onClick.AddListener(() => SetTab(InventoryTab.Fish));
        }

        if (furnitureTabButton != null)
        {
            furnitureTabButton.onClick.AddListener(() => SetTab(InventoryTab.Furniture));
        }

        if (materialTabButton != null)
        {
            materialTabButton.onClick.AddListener(() => SetTab(InventoryTab.Material));
        }

        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged += Refresh;
        }

        ClosePanelImmediate();
        SetTab(InventoryTab.Fish);
    }

    private void OnDestroy()
    {
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged -= Refresh;
        }
    }

    private void TogglePanel()
    {
        if (isOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    public void OpenPanel()
    {
        isOpen = true;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.blocksRaycasts = true;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.alpha = 1f;
        }

        StartSlide(openPosition);
        Refresh();
    }

    public void ClosePanel()
    {
        isOpen = false;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.interactable = false;
        }

        StartSlide(closedPosition);
    }

    private void ClosePanelImmediate()
    {
        isOpen = false;

        if (panelRoot != null)
        {
            panelRoot.anchoredPosition = closedPosition;
        }

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.alpha = 1f;
        }
    }

    private void StartSlide(Vector2 targetPosition)
    {
        if (slideRoutine != null)
        {
            StopCoroutine(slideRoutine);
        }

        slideRoutine = StartCoroutine(SlideRoutine(targetPosition));
    }

    private IEnumerator SlideRoutine(Vector2 targetPosition)
    {
        if (panelRoot == null)
        {
            yield break;
        }

        Vector2 startPosition = panelRoot.anchoredPosition;
        float time = 0f;

        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / slideDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            panelRoot.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        panelRoot.anchoredPosition = targetPosition;
    }

    private void SetTab(InventoryTab tab)
    {
        currentTab = tab;
        selectedSlot = null;
        Refresh();
    }

    private void Refresh()
    {
        ClearGrid();
        UpdateCapacityUI();

        if (currentTab == InventoryTab.Fish)
        {
            RefreshFishGrid();
        }
        else if (currentTab == InventoryTab.Furniture)
        {
            ShowPlaceholderDetail("가구", "가구 아이템은 다음 단계에서 연결합니다.");
        }
        else if (currentTab == InventoryTab.Material)
        {
            ShowPlaceholderDetail("재료", "재료 아이템은 다음 단계에서 연결합니다.");
        }
    }

    private void RefreshFishGrid()
    {
        if (inventorySystem == null || itemGridContent == null || slotPrefab == null)
        {
            return;
        }

        List<FishInventoryEntry> fishList = inventorySystem.GetAllFish();

        foreach (FishInventoryEntry entry in fishList)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, itemGridContent);
            slot.Setup(entry, OnSlotClicked);
        }

        if (fishList.Count > 0)
        {
            ShowFishDetail(fishList[0]);
        }
        else
        {
            ShowPlaceholderDetail("물고기 없음", "아직 잡은 물고기가 없습니다.");
        }
    }

    private void OnSlotClicked(FishInventoryEntry entry, InventorySlotUI slot)
    {
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
        }

        selectedSlot = slot;

        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(true);
        }

        ShowFishDetail(entry);
    }

    private void ShowFishDetail(FishInventoryEntry entry)
    {
        if (entry == null || entry.fish == null)
        {
            return;
        }

        if (detailIcon != null)
        {
            detailIcon.sprite = entry.fish.fishSprite;
            detailIcon.preserveAspect = true;
        }

        if (detailNameText != null)
        {
            detailNameText.text = entry.fish.fishName;
        }

        if (detailInfoText != null)
        {
            detailInfoText.text =
                $"등급: {entry.fish.rarity}\n" +
                $"보유 수량: {entry.count}\n" +
                $"보상 코인: {entry.fish.coinReward}\n" +
                $"ID: {entry.fish.fishId}";
        }
    }

    private void ShowPlaceholderDetail(string title, string body)
    {
        if (detailIcon != null)
        {
            detailIcon.sprite = null;
        }

        if (detailNameText != null)
        {
            detailNameText.text = title;
        }

        if (detailInfoText != null)
        {
            detailInfoText.text = body;
        }
    }

    private void UpdateCapacityUI()
    {
        if (inventorySystem == null)
        {
            return;
        }

        int used = inventorySystem.CapacityUsed;
        int max = inventorySystem.MaxCapacity;

        float ratio = max <= 0 ? 0f : (float)used / max;

        if (capacityText != null)
        {
            capacityText.text = $"{used} / {max}";
        }

        if (capacityFillImage != null)
        {
            capacityFillImage.fillAmount = ratio;

            if (ratio >= 1f)
            {
                capacityFillImage.color = fullColor;
            }
            else if (ratio >= 0.8f)
            {
                capacityFillImage.color = warningColor;
            }
            else
            {
                capacityFillImage.color = enoughColor;
            }
        }
    }

    private void ClearGrid()
    {
        if (itemGridContent == null)
        {
            return;
        }

        foreach (Transform child in itemGridContent)
        {
            Destroy(child.gameObject);
        }
    }
}
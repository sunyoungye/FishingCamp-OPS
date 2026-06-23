using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleFishingSystem : MonoBehaviour
{
    [Header("Fish Data")]
    public List<FishDataSO> fishList = new List<FishDataSO>();

    [Header("Managers")]
    public InventoryManager inventoryManager;
    public FishCaughtPopupUI fishCaughtPopupUI;

    [Header("Button UI")]
    public Button fishingButton;
    public Image fishingButtonImage;
    public Sprite fishingButtonSprite;
    public Sprite catchButtonSprite;

    [Header("Vertical Fishing Bar UI")]
    public GameObject verticalFishingBarPanel;
    public RectTransform waterFillArea;
    public RectTransform waterBodyRect;
    public RectTransform waterSurfaceRect;
    public Image targetZoneImage;
    public GameObject failedImage;

    [Header("Player")]
    public PlayerCtrl playerMovement;

    [Header("Fishing Mini Game Settings")]
    [Range(0f, 1f)]
    public float targetValue = 0.7f;

    [Range(0f, 0.5f)]
    public float successTolerance = 0.08f;

    private bool playerInFishingSpot = false;
    private bool isFishing = false;

    private float currentFillAmount = 0f;
    private FishDataSO currentFish;

    private Coroutine failedRoutine;

    private void Start()
    {
        ResetFishingState();

        if (fishingButton != null)
        {
            fishingButton.onClick.AddListener(OnFishingButtonClicked);
        }

        UpdateTargetZonePosition();
    }

    private void Update()
    {
        if (!isFishing) return;
        if (currentFish == null) return;

        currentFillAmount += currentFish.fillSpeed * Time.deltaTime;
        currentFillAmount = Mathf.Clamp01(currentFillAmount);

        UpdateWaterVisual();

        if (currentFillAmount >= 1f)
        {
            FailFishing();
        }
    }

    private void UpdateWaterVisual()
    {
        if (waterFillArea == null) return;
        if (waterBodyRect == null) return;
        if (waterSurfaceRect == null) return;

        float maxHeight = waterFillArea.rect.height;
        float currentHeight = maxHeight * currentFillAmount;

        waterBodyRect.sizeDelta = new Vector2(
            waterBodyRect.sizeDelta.x,
            currentHeight
            );

        waterSurfaceRect.anchoredPosition = new Vector2(
            waterSurfaceRect.anchoredPosition.x,
            currentHeight
            );
    }

    public void SetPlayerInFishingSpot(bool value)
    {
        playerInFishingSpot = value;

        if (value)
        {
            ShowFishingButton();
        }
        else
        {
            CancelFishingAndHide();
        }
    }

    private void OnFishingButtonClicked()
    {
        if (!playerInFishingSpot) return;

        if (!isFishing)
        {
            StartFishing();
        }
        else
        {
            TryCatchFish();
        }
    }

    private void StartFishing()
    {
        currentFish = GetRandomFish();

        if (currentFish == null)
        {
            Debug.LogWarning("No fish data found.");
            return;
        }

        isFishing = true;
        currentFillAmount = 0f;

        ResetWaterVisual();

        if (verticalFishingBarPanel != null)
        {
            verticalFishingBarPanel.SetActive(true);
        }

        if (failedImage != null)
        {
            failedImage.SetActive(false);
        }

        SetFishingButtonImage(true);

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }

        Debug.Log($"Fishing started. Hidden fish: {currentFish.fishName}, speed: {currentFish.fillSpeed}");
    }

    private void ResetWaterVisual()
    {
        currentFillAmount = 0f;

        if (waterBodyRect != null)
        {
            waterBodyRect.sizeDelta = new Vector2(
                waterBodyRect.sizeDelta.x,
                0f
                );
        }

        if (waterSurfaceRect != null)
        {
            waterSurfaceRect.anchoredPosition = new Vector2(
                waterSurfaceRect.anchoredPosition.x,
                0f
                );
        }
    }

    private void SetFishingButtonImage(bool isCatchMode)
    {
        if (fishingButtonImage == null) return;

        if (isCatchMode)
        {
            fishingButtonImage.sprite = catchButtonSprite;
        }
        else
        {
            fishingButtonImage.sprite = fishingButtonSprite;
        }

        fishingButtonImage.preserveAspect = true;
    }

    private void TryCatchFish()
    {
        if (!isFishing) return;

        float minSuccess = targetValue - successTolerance;
        float maxSuccess = targetValue + successTolerance;

        bool isSuccess = currentFillAmount >= minSuccess && currentFillAmount <= maxSuccess;

        if (isSuccess)
        {
            SuccessFishing();
        }
        else
        {
            FailFishing();
        }
    }

    private void SuccessFishing()
    {
        if (currentFish != null)
        {
            if (inventoryManager != null)
            {
                inventoryManager.AddFish(currentFish);

            }

            if (fishCaughtPopupUI != null)
            {
                fishCaughtPopupUI.Show(currentFish);
            }

            Debug.Log($@"
{{
    ""event"": ""fish_caught"",
    ""result"": ""success"",
    ""fishId"": ""{currentFish.fishId}"",
    ""fishName"": ""{currentFish.fishName}"",
    ""rarity"": ""{currentFish.rarity}"",
    ""coinReward"": {currentFish.coinReward},
    ""fillAmount"": {currentFillAmount}
}}");
        }

        EndFishingKeepButton();
    }

    private void FailFishing()
    {
        if (!isFishing) return;

        Debug.Log($@"
{{
    ""event"": ""fish_caught"",
    ""result"": ""success"",
    ""fishId"": ""{currentFish.fishId}"",
    ""fishName"": ""{currentFish.fishName}"",
    ""rarity"": ""{currentFish.rarity}"",
    ""coinReward"": {currentFish.coinReward},
    ""fillAmount"": {currentFillAmount}
}}");

        if (failedRoutine != null)
        {
            StopCoroutine(failedRoutine);
        }

        failedRoutine = StartCoroutine(ShowFailedImageRoutine());

        EndFishingKeepButton();
    }

    private IEnumerator ShowFailedImageRoutine()
    {
        if (failedImage != null)
        {
            failedImage.SetActive(true);
        }

        yield return new WaitForSeconds(1f);

        if (failedImage != null)
        {
            failedImage.SetActive(false);
        }
    }

    private void EndFishingKeepButton()
    {
        isFishing = false;
        currentFish = null;
        currentFillAmount = 0f;

        ResetWaterVisual();

        if (verticalFishingBarPanel != null)
        {
            verticalFishingBarPanel.SetActive(false);
        }

        SetFishingButtonImage(false);

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }

        if (playerInFishingSpot)
        {
            ShowFishingButton();
        }
    }

    private void CancelFishingAndHide()
    {
        isFishing = false;
        currentFish = null;
        currentFillAmount = 0f;

        ResetWaterVisual() ;

        if (verticalFishingBarPanel != null)
        {
            verticalFishingBarPanel.SetActive(false);
        }

        SetFishingButtonImage(false);

        if (fishingButton != null)
        {
            fishingButton.gameObject.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }
    }

    private void ShowFishingButton()
    {
        if (fishingButton != null)
        {
            fishingButton.gameObject.SetActive(true);
        }

        SetFishingButtonImage(isFishing);
    }

    private void ResetFishingState()
    {
        isFishing = false;
        currentFish = null;
        currentFillAmount = 0f;

        if (fishingButton != null)
        {
            fishingButton.gameObject.SetActive(false);
        }

        if (verticalFishingBarPanel != null)
        {
            verticalFishingBarPanel.SetActive(false);
        }

        ResetWaterVisual();

        if (failedImage != null)
        {
            failedImage.SetActive(false);
        }

        SetFishingButtonImage(false);
    }

    private void UpdateTargetZonePosition()
    {
        if (targetZoneImage == null) return;
        if (waterFillArea == null) return;

        RectTransform targetRect = targetZoneImage.GetComponent<RectTransform>();

        float fillHeight = waterFillArea.rect.height;

        float yPosition = Mathf.Lerp(
            -fillHeight / 2f,
            fillHeight / 2f,
            targetValue
            );

        targetRect.anchoredPosition = new Vector2(
            targetRect.anchoredPosition.x,
            yPosition
            );
    }

    private FishDataSO GetRandomFish()
    {
        if (fishList == null || fishList.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;

        foreach (FishDataSO fish in fishList)
        {
            totalWeight += fish.dropWeight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (FishDataSO fish in fishList)
        {
            currentWeight += fish.dropWeight;

            if (randomValue < currentWeight)
            {
                return fish;
            }
        }

        return fishList[0];
    }
}

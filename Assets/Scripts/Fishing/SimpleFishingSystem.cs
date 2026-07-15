using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;

public class SimpleFishingSystem : MonoBehaviour
{
    [Header("Fish Data")]
    public List<FishDataSO> fishList = new List<FishDataSO>();

    [Header("Managers")]
    public InventoryManager inventoryManager;
    public CurrencyManager currencyManager;
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

    [Header("Result Timing")]
    public float successAnimationDelay = 0.8f;
    public float failAnimationDelay = 0.8f;

    [Header("Fishing Mini Game Settings")]
    [Range(0f, 1f)]
    public float targetValue = 0.7f;

    [Range(0f, 0.5f)]
    public float successTolerance = 0.08f;

    private bool playerInFishingSpot = false;
    private bool IsFishing = false;

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
        if (!IsFishing) return;
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
        if (!playerInFishingSpot)
        {
            return;
        }

        if (!IsFishing)
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
        Debug.Log("StartFishing called.");

        currentFish = GetRandomFish();

        if (currentFish == null)
        {
            Debug.LogWarning("No fish data found.");
            return;
        }

        IsFishing = true;
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

        SetFishingButtonImage(true); // Change button image as "Catch"

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
            playerMovement.SetFishingAnimation(true);
        }

        Debug.Log("Fishing animation started.");
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
        if (!IsFishing) return;

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
        if (!IsFishing) return;

        StartCoroutine(SuccessFishingRoutine());
    }

    private IEnumerator SuccessFishingRoutine()
    {
        IsFishing = false;
        bool added = false;

        FishDataSO caughtFish = currentFish;

        if (fishingButton != null)
        {
            fishingButton.gameObject.SetActive(false);
        }

        if (verticalFishingBarPanel != null)
        {
            verticalFishingBarPanel.gameObject.SetActive(false);
        }

        ResetWaterVisual();

        if (playerMovement != null)
        {
            playerMovement.PlayFishingSuccess();
        }

        if (caughtFish != null)
        {
            if (inventoryManager != null)
            {
                added = inventoryManager.AddFish(currentFish);
            }

            if (added && currencyManager != null && currentFish != null)
            {
                currencyManager.AddCoins(currentFish.coinReward);
            }

            Debug.Log($"Fishing Success: {caughtFish.fishName}");
        }

        yield return new WaitForSeconds(successAnimationDelay);

        if (playerMovement != null)
        {
            playerMovement.SetFishingAnimation(false);
        }

        if (caughtFish != null && fishCaughtPopupUI != null)
        {
            yield return StartCoroutine(fishCaughtPopupUI.ShowAndWait(caughtFish));
        }

        EndFishingKeepButton();
    }
    private void FailFishing()
    {
        if (!IsFishing) return;

        StartCoroutine(FailFishingRoutine());
    }

    private IEnumerator FailFishingRoutine()
    {
        IsFishing = false;

        if (fishingButton != null)
        {
            fishingButton.gameObject.SetActive(false);
        }

        if (verticalFishingBarPanel != null)
        {
            verticalFishingBarPanel.SetActive(false);
        }

        ResetWaterVisual();

        if (playerMovement != null)
        {
            playerMovement.PlayFishingFail();
        }

        if (failedImage != null)
        {
            failedImage.SetActive(true);
        }

        Debug.Log("Fishing Failed.");

        yield return new WaitForSeconds(failAnimationDelay);

        if (failedImage != null)
        {
            failedImage.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.SetFishingAnimation(false);
        }

        EndFishingKeepButton();
    }

    private IEnumerator EndFishingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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
            playerMovement.SetFishingAnimation(false);
            playerMovement.SetCanMove(true);
        }

        if (playerInFishingSpot && fishingButton != null)
        {
            fishingButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator EndFishingAfterResultDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndFishingKeepButton();
    }

    private void CancelFishingAndHide()
    {
        IsFishing = false;
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

        if (failedImage != null)
        {
            failedImage.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.SetFishingAnimation(false);
            playerMovement.SetCanMove(true);
        }
    }

    private void ShowFishingButton()
    {
        if (fishingButton != null)
        {
            fishingButton.gameObject.SetActive(true);
        }

        SetFishingButtonImage(IsFishing);
    }

    private void ResetFishingState()
    {
        IsFishing = false;
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

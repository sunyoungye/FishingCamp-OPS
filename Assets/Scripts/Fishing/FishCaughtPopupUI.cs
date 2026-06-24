using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishCaughtPopupUI : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public Image fishImage;
    public TMP_Text rarityText;
    public TMP_Text fishNameText;
    public TMP_Text rewardText;

    [Header("Animation")]
    public float fadeDuration = 0.35f;
    public float stayDuration = 1.5f;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public void Show(FishDataSO fish)
    {
        if (fish == null)
        {
            Debug.LogWarning("FishCaughtPopupUI.Show called with null fish");
            return;
        }

        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(ShowRoutine(fish));
    }

    public IEnumerator ShowAndWait(FishDataSO fish)
    {
        if (fish == null) yield break;

        gameObject.SetActive(true);
        StopAllCoroutines();

        yield return StartCoroutine(ShowRoutine(fish));
    }

    private IEnumerator ShowRoutine(FishDataSO fish)
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup is not assigned on FishCaughtPopup UI.");
            yield break;
        }

        if (fishImage != null)
        {
            fishImage.sprite = fish.fishSprite;
            fishImage.preserveAspect = true;
        }

        if (rarityText != null)
        {
            rarityText.text = fish.rarity.ToString();
        }

        if (fishNameText != null)
        {
            fishNameText.text = fish.fishName;
        }

        if (rewardText != null)
        {
            rewardText.text = $"+{fish.coinReward} Coin";
        }

        canvasGroup.alpha = 0f;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp(0f, 1f, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(stayDuration);

        time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
    }
}

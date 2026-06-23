using System.Collections;
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

    [Header("Fade Settings")]
    public float fadeDuration = 0.4f;
    public float stayDuration = 1.5f;

    public void Show(FishDataSO fish)
    {
        if (fish == null) return;

        StopAllCoroutines();
        StartCoroutine(ShowRoutine(fish));
    }

    private IEnumerator ShowRoutine(FishDataSO fish)
    {
        gameObject.SetActive(true);

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

        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup is not assigned.");
            yield break;
        }

        canvasGroup.alpha = 0f;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
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
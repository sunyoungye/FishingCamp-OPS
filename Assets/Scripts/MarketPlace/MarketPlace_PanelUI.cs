using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MarketPlace_PanelUI : MonoBehaviour
{
    [Header("Panel Side")]
    public RectTransform panelRoot;
    public CanvasGroup panelCanvasGroup;
    public Button marketPlaceButton;
    public Button closeButton;

    public Vector2 openPosition = new Vector2(0f, 1f);
    public Vector2 closePosition = new Vector2(1440f, 0f);
    public float slideDuration = 0.75f;

    private bool isOpen = false;
    private Coroutine slideRoutine;

    public void Start()
    {
        if (marketPlaceButton != null)
        {
            marketPlaceButton.onClick.AddListener(ToggleDown);
        }
    }

    private void ToggleDown()
    {
        if (isOpen)
        {
            CloseMarketPlace();
        }
        else
        {
            OpenMarketPlace();
        }
    }

    private void OpenMarketPlace()
    {
        isOpen = true;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.blocksRaycasts = true;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.alpha = 1f;
        }

        StartSlide(openPosition);
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

        while(time < slideDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / slideDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            panelRoot.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
    }

    private void CloseMarketPlace()
    {
        isOpen = false;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.interactable = false;
        }

        StartSlide(closePosition);
    }

    public void OpenMarket()
    {
        isOpen = true;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.blocksRaycasts = true;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.alpha = 1f;
        }

        StartSlide(openPosition);
    }
}

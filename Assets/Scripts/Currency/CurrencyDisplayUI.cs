using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CurrencyDisplayUI : MonoBehaviour
{
    [Header("References")]
    public CurrencyManager currencyManager;

    [Header("Texts")]
    public TMP_Text coinText;
    public TMP_Text rubyText;

    private void Start()
    {
        if (currencyManager != null)
        {
            currencyManager.OnCoinsChanged += UpdateCoinText;
            currencyManager.OnRubiesChanged += UpdateRubyText;

            UpdateCoinText(currencyManager.Coins);
            UpdateRubyText(currencyManager.Rubies);
        }
        else
        {
            Debug.LogWarning("CurrencyManager is not assigned.");
        }
    }

    private void OnDestroy()
    {
        if (currencyManager != null)
        {
            currencyManager.OnCoinsChanged -= UpdateCoinText;
            currencyManager.OnRubiesChanged -= UpdateRubyText;
        }
    }

    private void UpdateCoinText(int amount)
    {
        if (coinText != null)
        {
            coinText.text = $"Coin: {amount}";
        }
    }

    private void UpdateRubyText(int amount)
    {
        if (rubyText != null)
        {
            rubyText.text = $"Ruby: {amount}";
        }
    }
}

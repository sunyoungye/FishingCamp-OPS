using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int startingCoins = 1000;
    [SerializeField] private int coins;

    [Header("Ruby Settings")]
    [SerializeField] private int rubies;
    private const string RubySaveKey = "PLAYER_RUBY";

    public int Coins => coins;
    public int Rubies => rubies;

    public event Action<int> OnCoinsChanged;
    public event Action<int> OnRubiesChanged;

    private void Awake()
    {
        LoadRubies();

        coins = startingCoins;
    }

    private void Start()
    {
        NotifyAll();
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        coins += amount;
        OnCoinsChanged?.Invoke(coins);

        Debug.Log($"Coins Added: +{amount}, Total: {coins}");
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (coins < amount)
        {
            Debug.LogWarning("Not enough coins.");
            return false;
        }

        coins -= amount;
        OnCoinsChanged?.Invoke(coins);

        Debug.Log($"Coins Spent: -{amount}, Total: {coins}");
        return true;
    }

    public void ResetSessionCoins(int newAmount = 0)
    {
        coins = Mathf.Max(0, newAmount);
        OnCoinsChanged?.Invoke(coins);

        Debug.Log($"Session Coins Reset: {coins}");
    }

    public void AddRubies(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        rubies += amount;
        SaveRubies();
        OnRubiesChanged?.Invoke(rubies);

        Debug.Log($"Rubies Added: +{amount}, Total: {rubies}");
    }

    public bool SpendRubies(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (rubies < amount)
        {
            Debug.LogWarning("Not enough rubies.");
            return false;
        }

        rubies -= amount;
        SaveRubies();
        OnRubiesChanged?.Invoke(rubies);

        Debug.LogWarning($"Rubies Spent: -{amount}, Total: {rubies}");
        return true;
    }

    private void LoadRubies()
    {
        rubies = PlayerPrefs.GetInt(RubySaveKey, 0);    
    }

    private void SaveRubies()
    {
        PlayerPrefs.SetInt(RubySaveKey, rubies);
        PlayerPrefs.Save();
    }

    private void NotifyAll()
    {
        OnCoinsChanged?.Invoke(coins);
        OnCoinsChanged?.Invoke(rubies);
    }
}

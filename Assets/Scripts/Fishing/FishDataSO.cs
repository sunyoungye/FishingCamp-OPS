using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FishRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}


[CreateAssetMenu(fileName = "NewFishData", menuName = "Fishing/Fish Data")]
public class FishDataSO : ScriptableObject
{
    public string fishId;
    public string fishName;
    public FishRarity rarity;
    public Sprite fishSprite;
    public int coinReward;
    public int dropWeight;

    [Header("Fishing Mini Game")]
    public float fillSpeed = 0.25f;
}

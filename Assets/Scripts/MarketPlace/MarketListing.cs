using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MarketListing
{
    public string listingId;

    public FishDataSO fish;
    public int quantity;
    public int price;

    public string sellerName;
    public float remainingTime;

    public bool isSold;
    public bool isPlayerListing;
}

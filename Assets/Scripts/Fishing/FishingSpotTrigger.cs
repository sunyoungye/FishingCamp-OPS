using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingSpotTrigger : MonoBehaviour
{
    public SimpleFishingSystem fishingSystem;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (fishingSystem != null)
            {
                fishingSystem.SetPlayerInFishingSpot(true);
            }

            Debug.Log("Player entered fishing spot.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (fishingSystem != null)
            {
                fishingSystem.SetPlayerInFishingSpot(false);
            }

            Debug.Log("Player left fishing spot.");
        }
    }
}

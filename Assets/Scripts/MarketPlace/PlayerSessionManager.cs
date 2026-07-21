using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSessionManager : MonoBehaviour
{
    public string playerId;
    public string playerName;
    public bool isHOst;

    private void Awake()
    {
        if (string.IsNullOrEmpty(playerId))
        {
            playerId = System.Guid.NewGuid().ToString();
        }

        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player";
        }
    }
}

using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerOwnerSetup : NetworkBehaviour
{
    [Header("Owner Only Behaviours")]
    public MonoBehaviour[] ownerOnlyBehaviours;

    [Header("Owner Only Objects")]
    public GameObject[] ownerOnlyObjects;

    [Header("Physics")]
    public Rigidbody2D rb2D;

    [Header("Spawn Test")]
    public bool randomizeSpawnPosition = true;
    public float spawnRangeX = 2f;
    public float spawnRangeY = 1f;

    public override void OnNetworkSpawn()
    {
        bool isMine = IsOwner;

        gameObject.name = isMine ? "NetworkPlayer_LocalOwner" : "NetworkPlayer_Remote";

        foreach (MonoBehaviour behaviour in ownerOnlyBehaviours)
        {
            if (behaviour != null)
            {
                behaviour.enabled = isMine;
            }
        }

        foreach (GameObject obj in ownerOnlyObjects)
        {
            if (obj != null)
            {
                obj.SetActive(isMine);
            }
        }

        if (!isMine && rb2D != null)
        {
            rb2D.simulated = false;
        }

        if (isMine && randomizeSpawnPosition)
        {
            Vector3 pos = transform.position;
            pos.x += Random.Range(-spawnRangeX, spawnRangeY);
            pos.y += Random.Range(-spawnRangeY, spawnRangeY);
            transform.position = pos;
        }

        Debug.Log($"Network Player Spawned. IsOwner: {IsOwner}, IsHost: {IsHost}, ClientId: {OwnerClientId}");
    }
}

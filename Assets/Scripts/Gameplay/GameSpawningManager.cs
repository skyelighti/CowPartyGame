using System.Collections;
using System.Runtime.Versioning;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class GameSpawningManager : NetworkBehaviour
{
    public static GameSpawningManager Instance { get; private set; }
    private UnityEngine.Vector3 spawnpos;

    [SerializeField] private GameObject playerPrefab;
    public GameObject[] farmerMeshPrefab;
    public GameObject[] alienMeshPrefab;
    public GameObject trailPrefab;

    /// <summary>
    /// playerControllerOfId[id] returns the PlayerController that corresponds to the client ID 'id'. 
    /// Only usable on the server.
    /// </summary>
    public Dictionary<ulong, PlayerController> playerControllerOfId = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject FarmerSpawn;
    public GameObject[] AlienSpawns;
    [SerializeField] public GameObject player;
    public void SpawnPlayers(List<NetworkedPlayerGameplayInfo> networkedPlayerGameplayInfos)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Non-server called SpawnPlayers function. Only the server should spawn players.");
            return;
        }

        foreach (NetworkedPlayerGameplayInfo networkedPlayerGameplayInfo in networkedPlayerGameplayInfos)
        {
            if (networkedPlayerGameplayInfo.isAlien)
            {
                Vector3 alienSpawnPlatformPos = AlienSpawns[0].transform.position;
                spawnpos = new(alienSpawnPlatformPos.x + networkedPlayerGameplayInfo.id, alienSpawnPlatformPos.y, alienSpawnPlatformPos.z);
                player = Instantiate(playerPrefab, spawnpos, Quaternion.identity);
            }
            else
            {
                Vector3 farmerSpawnPlatformPos = FarmerSpawn.transform.position;
                spawnpos = new(farmerSpawnPlatformPos.x + networkedPlayerGameplayInfo.id, farmerSpawnPlatformPos.y, farmerSpawnPlatformPos.z);
                player = Instantiate(playerPrefab, spawnpos, Quaternion.identity);
            }
            NetworkObject networkObject = player.GetComponent<NetworkObject>();
            playerControllerOfId[networkedPlayerGameplayInfo.id] = player.GetComponent<PlayerController>();
            networkObject.SpawnAsPlayerObject(networkedPlayerGameplayInfo.id, true);
        }
    }
}
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class HaybaleManager : NetworkBehaviour
{
    [SerializeField] private GameObject haybalePrefab;

    [SerializeField] private BoxCollider spawnRegion;
    [SerializeField] private int initialHaybaleCount;
    [SerializeField] private float haybaleSpawnDelay;

    private float lastSpawnTime;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }
        for (int i = 0; i < initialHaybaleCount; i++)
        {
            SpawnHaybale();
        }
    }

    void Update()
    {
        if (!IsServer) { return; }
        if (Time.time >= lastSpawnTime + haybaleSpawnDelay)
        {
            SpawnHaybale();
            lastSpawnTime = Time.time;
        }
    }

    public void SpawnHaybale()
    {
        GameObject newObj = Instantiate(haybalePrefab, GetRandomSpawnPoint(spawnRegion.bounds), Quaternion.identity);
        NetworkObject netObj = newObj.GetComponent<NetworkObject>();
        netObj.Spawn(true);
    }

    public Vector3 GetRandomSpawnPoint(Bounds bounds)
    {
        Vector3 pointToSample = new(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.min.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
        if (!NavMesh.SamplePosition(pointToSample, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            Debug.LogWarning("NavMesh.SamplePosition in GetRandomSpawnPoint of HaybaleManager found no valid point! Returning the sampled position.");
            return pointToSample;
        }
        return hit.position;
    }
}

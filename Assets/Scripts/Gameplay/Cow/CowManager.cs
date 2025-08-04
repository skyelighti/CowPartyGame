using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class CowManager : MonoBehaviour
{
    public static CowManager Instance { get; private set; }

    [SerializeField] private GameObject cowPrefab;
    [SerializeField] private Transform cowOriginTransform;
    [SerializeField] private BoxCollider spawnBoxCollider;
    [SerializeField] private BoxCollider wanderBoxCollider;
    [SerializeField] private float maxSampleDistance;

    public Bounds DefaultWanderBounds => wanderBoxCollider.bounds;
    [SerializeField] private Bounds afterFallNewWanderBounds;
    [SerializeField] private float xPosOfNewWanderBounds = 209.5f;

    public float noPathFoundAllowedDuration;
    public float waitTime;
    public float distanceToGroundToStopFalling;
    public float falingToGroundStateMaxTime;
    /// <summary>
    /// When cows fall, a force of this magnitude is applied on them toward the CowOrigin after every bounce
    /// </summary>
    public float toCowOriginForce;
    public PhysicsMaterial bouncyMaterial;
    public Vector3 CowOrigin => cowOriginTransform.position;

    public List<CowController> cowControllers = new();

    public int cowsAtGameStart;
    public int cowsRemaining;
    public int CowsAbducted => cowsAtGameStart - cowsRemaining;
    public int winconditionPercentageCows;

    /// <summary>
    /// Only filled in on the server
    /// </summary>
    public Dictionary<ulong, GameObject> cows = new();

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

    // Only called on the server
    public void SpawnCows(int cows)
    {
        cowsRemaining = cowsAtGameStart = cows;
        for (int i = 0; i < cows; i++)
        {
            GameObject newCow = Instantiate(cowPrefab, GetRandomSpawnPoint(), Quaternion.identity);
            this.cows[(ulong)i] = newCow;

            CowController cowController = newCow.GetComponent<CowController>();
            cowController.id.Value = (ulong)i;
            cowControllers.Add(cowController);

            NetworkObject networkObject = newCow.GetComponent<NetworkObject>();
            networkObject.Spawn();
        }
    }

    public void FreeCow(ulong id)
    {
        cows[id].GetComponent<ServerCowController>().FailAbduction();
    }

    public void CowInMothership(ulong id)
    {
        cowsRemaining--;
        cows[id].GetComponent<NetworkObject>().Despawn();
        NetworkedGameplayManager.Instance.OnCowInMothership();
    }

    public Vector3 GetRandomSpawnPoint()
    {
        Bounds bounds = spawnBoxCollider.bounds;
        return GetRandomNavMeshPointInBounds(bounds);
    }

    public Vector3 GetRandomNavMeshPointInBounds(Bounds bounds)
    {
        Vector3 pointToSample = new(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
        NavMesh.SamplePosition(pointToSample, out NavMeshHit hit, maxSampleDistance, NavMesh.AllAreas);
        return hit.position;
    }

    public Bounds GetNewWanderBoundsAtPos(Vector3 center)
    {
        center.x = xPosOfNewWanderBounds;
        return new Bounds(center, afterFallNewWanderBounds.size);
    }

    public Vector3 GetAverageCowPos()
    {
        Vector3 res = Vector3.zero;
        foreach (CowController cowController in cowControllers)
        {
            res += cowController.transform.position;
        }
        res /= cowControllers.Count;
        return res;
    }
}

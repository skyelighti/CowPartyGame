using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class Haybale : NetworkBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private float lifetime;

    private NetworkObject net;

    [SerializeField] private float timeBetweenScans;
    private float lastScanTime;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
        }
        Invoke(nameof(Remove), lifetime);

        lastScanTime = float.MinValue;
        net = GetComponent<NetworkObject>();
        base.OnNetworkSpawn();
    }
    void Start()
    {
        NetSfx.Play(SfxId.haybale, transform.position, 1);//place haybale sound
    }
    void Update()
    {
        if (Time.time >= lastScanTime + timeBetweenScans)
        {
            ScanForCows();
            lastScanTime = Time.time;
        }
    }

    void ScanForCows()
    {
        foreach (CowController cowController in CowManager.Instance.cowControllers)
        {
            ServerCowController serverCowController = cowController.serverCowController;
            if (Vector3.Distance(transform.position, serverCowController.transform.position) <= radius)
            {
                serverCowController.StartFollowing(transform);
            }
        }
    }

    private void Remove()
    {
        foreach (CowController cowController in CowManager.Instance.cowControllers)
        {
            ServerCowController serverCowController = cowController.serverCowController;
            if (Vector3.Distance(transform.position, serverCowController.transform.position) <= radius)
            {
                serverCowController.StopFollowing(transform);
            }
        }
        NetSfx.Play(SfxId.haybale, transform.position, 2);//destroy haybale sound
        net.Despawn();
    }
}

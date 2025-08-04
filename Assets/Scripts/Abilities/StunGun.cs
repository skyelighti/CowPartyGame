using System.Threading;
using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Fragsurf.Movement;

public class StunGun : GunBase
{
    [SerializeField] private float stunDur;
    [SerializeField] private GameObject StunVFX;
    public override void ServerGunOnHit()
    {
        if (hit.transform.gameObject.TryGetComponent(out PlayerController controller))
        {
            controller.StartStun(stunDur);
            StunVFXRpc();
        }
    }
    [Rpc(SendTo.Everyone)]
    private void StunVFXRpc()
    { 
        var impact = Instantiate(StunVFX, hit.transform.gameObject.transform.position, Quaternion.identity);
        var impactnet = impact.GetComponent<NetworkObject>();
        impactnet.Spawn();
    }

    void Start()
    {
        //gunuse = true;
        player = gameObject.GetComponent<PlayerController>();
    }
}

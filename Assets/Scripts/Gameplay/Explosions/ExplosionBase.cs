using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class ExplosionBase : NetworkBehaviour
{
    [SerializeField] protected int damage;
    [SerializeField] protected float lifetime;
    protected List<GameObject> alreadyHit;

    public abstract void OnHit(Collider hit);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        Lifetime();
    }

    void OnTriggerEnter(Collider hitPt)
    {
        if (!IsServer) return;
        OnHit(hitPt);
    }

    IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(this.gameObject);
    }
}

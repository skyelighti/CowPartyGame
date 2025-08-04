using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class BulletBase : NetworkBehaviour
{
    //is instantiated with correct orientation and location
    [SerializeField] protected int damage;
    [SerializeField] protected float speed;
    [SerializeField] protected float lifetime;

    protected Rigidbody rb;

    public abstract void Flying();
    public abstract void OnHit(Collider hit);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        rb = GetComponent<Rigidbody>();
        Flying();
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

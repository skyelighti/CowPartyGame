using System;
using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class VFX : NetworkBehaviour
{
    [SerializeField] private float Life = 1f;
    void Start()
    {
        //DeleteTrailServerRpc();
    }

    // Update is called once per frame
    void Update()
    {

    }
    [ServerRpc(RequireOwnership = false)]
    public void DeleteTrailServerRpc()
    {
        if (IsServer)
        { 
            StartCoroutine(Lifespan());
        }   
    }

    IEnumerator Lifespan()
    {
        yield return new WaitForSeconds(Life);
        NetworkManager.Destroy(gameObject);
    }
    public override void OnNetworkSpawn()
    {
        DeleteTrailServerRpc();
    }
}

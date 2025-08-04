using System;
using System.Collections;
using UnityEngine;
using Fragsurf.Movement;
using Unity.Netcode;

public class Dash : PlayerAbilties
{
    public GameObject DashFX;
    private NetworkedPlayerGameplayInfo playerInfo;
    SurfCharacter surfChar;
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }
        rb = gameObject.GetComponent<Rigidbody>();
    }
    [ServerRpc(RequireOwnership = false)]
    public void SpawnEffectServerRpc()
    {
        //Debug.Log("serverrpcdashtrailsapwned");
        var trail = Instantiate(DashFX, transform.position, transform.localRotation);
        var trailnet = trail.GetComponent<NetworkObject>();
        if (trailnet != null)
        {
            trailnet.Spawn();
        }
        else
        {
            Debug.LogError("DashFX prefab is missing a NetworkObject");
        }
    }
    public override void Ability()
    {
        //Debug.Log($"[Dash] IsSpawned: {IsSpawned}, IsOwner: {IsOwner}, NetworkObject: {GetComponent<NetworkObject>() != null}");
        surfChar = gameObject.GetComponent<SurfCharacter>();
        if (surfChar != null)
        {
            //Vector3 forceDir = (surfChar.transform.position - transform.position).normalized;
            surfChar.ApplyImpulse(transform.forward, 100f);
            StartCoroutine(NoCamRaycastWhileDashing());

            NetSfx.Play(SfxId.abilities, transform.position);
            SpawnEffectServerRpc();
            Debug.Log("boom boom");
        }
    }

    IEnumerator NoCamRaycastWhileDashing()
    {
        CameraSystemManager.Instance.useRaycast = false;
        yield return new WaitUntil(() => surfChar.moveData.velocity.magnitude <= 12f);
        CameraSystemManager.Instance.useRaycast = true;
    }
}

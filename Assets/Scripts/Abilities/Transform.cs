using System;
using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class TransformPlayer : PlayerAbilties
{
    public GameObject FarmerMesh;
    [SerializeField] private GameObject AlienMesh;
    [SerializeField] GameObject farmer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Debug.Log("isNotOwner");
            return;
        }
        else
        {
            Debug.Log("isOwnertransform");
        }

    }

    public override void Ability()
    {
        AlienMesh = GetComponentInChildren<Animator>().gameObject;
        AlienMesh.SetActive(false);
        ChangeMeshServerRpc();
        StartCoroutine(Countdown());
        Debug.Log("Ability called");

    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeMeshServerRpc()
    {
        Debug.Log("RPC Called");
        farmer = Instantiate(FarmerMesh);
        //need to reset farmer coords
        //enabling and disabling mes isnt working properly
        farmer.GetComponent<NetworkObject>().Spawn();
        farmer.transform.SetParent(transform);
        farmer.transform.localPosition = new Vector3(0,-0.15f,0);
        AlienMesh.SetActive(false);
        StartCoroutine(Countdown());

    }
    IEnumerator Countdown()
    {
        Debug.Log("Cooldown started");
        yield return new WaitForSeconds(10f);
        AlienMesh.SetActive(true);
        farmer.GetComponent<NetworkObject>().Despawn();
    }
}

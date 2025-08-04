using UnityEngine;
using Fragsurf.Movement;
using Unity.Netcode;
using System;


public class PlaceJumpPad : PlayerAbilties
{
    private NetworkedPlayerGameplayInfo playerInfo;
    [SerializeField] private Transform jumpPadPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }
    }

    public override void Ability()
    {
        Transform jumpPad = Instantiate(jumpPadPrefab);
        jumpPad.transform.position = transform.position-transform.up;

        JumpPad jumpPadScript = jumpPad.GetComponent<JumpPad>();
        if (transform.tag == "Alien")
            jumpPadScript.selectedTeam = JumpPad.allowedTeams.Aliens;
        else
            jumpPadScript.selectedTeam = JumpPad.allowedTeams.Farmers;

        jumpPadScript.UpdateMaterials();
    }
}

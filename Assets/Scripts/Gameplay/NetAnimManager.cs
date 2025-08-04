using System;
using UnityEngine;
using Unity.Netcode;

public class NetAnimManager : NetworkBehaviour
{
    public NetworkVariable<bool> isWalking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isAttacking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isShooting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isJumping = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        isWalking.OnValueChanged += WalkVal;
        isAttacking.OnValueChanged += AttackAnim;
        isShooting.OnValueChanged += Shooting;
        isJumping.OnValueChanged += Jumping;
        anim = GetComponentInChildren<Animator>();
    }
    private void WalkVal(bool previous, bool current)
    {
        anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetBool("isWalking", isWalking.Value);
        }

    }
    private void AttackAnim(bool previous, bool current)
    {
        anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetBool("isAttacking", isAttacking.Value);
        }
    }
    private void Shooting(bool previous, bool current)
    {
        anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetBool("Shooting", isShooting.Value);
        }
    }
    private void Jumping(bool previous, bool current)
    { 
        anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetBool("isJumping", isJumping.Value);
        }
    }
}

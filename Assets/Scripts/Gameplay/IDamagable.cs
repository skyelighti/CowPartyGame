using System;
using UnityEngine;
using Unity.Netcode;

public interface IDamageable
{
    //public NetworkVariable<int> Health { get;}
    public bool OnHit(ulong sourcePlayerId, int damage);
}

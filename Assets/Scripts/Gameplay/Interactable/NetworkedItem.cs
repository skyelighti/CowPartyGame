using Unity.Netcode;
using UnityEngine;

public class NetworkedItem : NetworkBehaviour
{
    public NetworkVariable<bool> isHeld = new(false);
    public NetworkVariable<ulong> heldId = new(ulong.MaxValue);
    [SerializeField] private float yAboveHolder;
    [SerializeField] private float throwForce;
    protected NetworkObject networkObject;
    protected Transform targetTransform;
    protected Rigidbody rb;

    /// <summary>
    /// Only known to the server
    /// </summary>
    protected bool wasThrown;

    public override void OnNetworkSpawn()
    {
        networkObject = GetComponent<NetworkObject>();
        rb = GetComponent<Rigidbody>();
        base.OnNetworkSpawn();
    }

    [Rpc(SendTo.Server)]
    public void AttemptToHoldRpc(NetworkedPlayerGameplayInfo networkedPlayerGameplayInfo)
    {
        if (isHeld.Value)
        {
            if (networkedPlayerGameplayInfo.id == heldId.Value)
            {
                Debug.LogWarning("A player tried to hold an item they were already holding!");
            }
            else
            {
                InformOfFailedToHoldRpc(networkedPlayerGameplayInfo.BaseRpcTarget);
            }
        }
        else
        {
            rb.useGravity = false;
            isHeld.Value = true;
            heldId.Value = networkedPlayerGameplayInfo.id;
            targetTransform = GameSpawningManager.Instance.playerControllerOfId[networkedPlayerGameplayInfo.id].transform;
        }
    }

    [Rpc(SendTo.Server)]
    public void AttemptToStopHoldRpc(Vector3 lookDir, NetworkedPlayerGameplayInfo networkedPlayerGameplayInfo)
    {
        if (!isHeld.Value)
        {
            Debug.LogWarning("A player attempted to stop holding an object that wasn't being held!");
            return;
        }
        if (heldId.Value != networkedPlayerGameplayInfo.id)
        {
            Debug.LogWarning("A player attempted to stop holding an object that was being held by someone else!");
            return;
        }
        rb.useGravity = true;
        Debug.Log(lookDir * throwForce);
        rb.AddForce(lookDir * throwForce, ForceMode.Impulse);
        wasThrown = true;
        isHeld.Value = false;
        heldId.Value = ulong.MaxValue;
        targetTransform = null;
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void InformOfFailedToHoldRpc(RpcParams rpcParams)
    {
        Debug.Log("Attempt to hold failed.");
    }

    private void Update()
    {
        if (targetTransform != null)
        {
            transform.position = targetTransform.position + yAboveHolder * Vector3.up;
        }
    }
}

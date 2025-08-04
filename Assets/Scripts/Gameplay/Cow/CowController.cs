using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The controller for cows that exists on all devices
/// </summary>
public class CowController : NetworkBehaviour
{
    [SerializeField] private GameObject visual;
    [SerializeField] private Collider col;
    [SerializeField] private CowAbductionInteractable interactable;

    // Null on non-server
    public ServerCowController serverCowController;

    public NetworkVariable<ulong> id = new();

    public CowInteractableIcon InteractableIcon => interactable.CowInteractableIcon;

    public bool FlaggedToAbduct { get; private set; }

    // Only set on the server
    private float flagToAbductTime;
    private float maximumFlagToAbductTime = 8f;

    void Start()
    {
        if (IsServer)
        {
            serverCowController = gameObject.AddComponent<ServerCowController>();
            serverCowController.cowController = this;
        }
        else
        {
            CowManager.Instance.cowControllers.Add(this);
        }
    }

    private void Update()
    {
        if (IsServer && FlaggedToAbduct && Time.time >= flagToAbductTime + maximumFlagToAbductTime)
        {
            SetAbductionFlagStatusRpc(false);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void DisableVisualsRpc()
    {
        visual.SetActive(false);
    }

    [Rpc(SendTo.Everyone)]
    public void EnableVisualsRpc()
    {
        visual.SetActive(true);
    }

    [Rpc(SendTo.Everyone)]
    public void SetBouncyRpc(bool bouncy)
    {
        col.material = bouncy ? CowManager.Instance.bouncyMaterial : null;
    }

    [Rpc(SendTo.Everyone)]
    public void SetAbductionFlagStatusRpc(bool flagged)
    {
        FlaggedToAbduct = flagged;
        if (flagged)
        {
            if (IsServer)
            {
                flagToAbductTime = Time.time;
            }
            if (NetworkManager.Singleton.LocalClient.PlayerObject.tag.Equals("Alien"))
                InteractableIcon.ShowUfo();
        }
        else
        {
            InteractableIcon.Hide();
        }
    }
}

using UnityEngine;

public class ItemInteractable : Interactable
{
    private Camera cam;
    private NetworkedItem netItem;

    private void Start()
    {
        cam = Camera.main;
        netItem = GetComponent<NetworkedItem>();
    }

    protected override bool PlayerControllerIsValid(LocalPlayerController localPlayerController)
    {
        return !netItem.isHeld.Value;
    }

    public override void Interact()
    {
        netItem.AttemptToHoldRpc(validLocalPlayerController.NetworkedPlayerGameplayInfo);
        base.Interact();
    }

    public override void StopInteract()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("This shouldn't happen! The cam was null and couldn't be retreived!");
                return;
            }
            else
            {
                Debug.LogWarning("This shouldn't happen! The cam was null, but could be found!");
            }
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.Log(ray.direction);
        netItem.AttemptToStopHoldRpc(ray.direction, validLocalPlayerController.NetworkedPlayerGameplayInfo);
        base.StopInteract();
    }
}

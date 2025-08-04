using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Interactable : MonoBehaviour
{
    [SerializeField] protected GameObject gameObjectWithInteractable;
    public IInteractableIcon interactableIcon;

    protected virtual void Start()
    {
        interactableIcon = gameObjectWithInteractable.GetComponent<IInteractableIcon>();
    }

    public virtual void Interact()
    {
        if (interactableIcon != null)
        {
            interactableIcon.OnInteract(validLocalPlayerController);
        }
        else
        {
            Debug.LogWarning("Interactable icon is null");
        }
        localPlayerIsInteracting = true;
    }

    public virtual void StopInteract()
    {
        if (interactableIcon != null)
        {
            interactableIcon.OnStopInteract(validLocalPlayerController);
        }
        else
        {
            Debug.LogWarning("Interactable icon is null");
        }
        localPlayerIsInteracting = false;
    }

    protected LocalPlayerController validLocalPlayerController;
    protected bool localPlayerIsInteracting;
    public bool LocalPlayerIsInteracting => localPlayerIsInteracting;
    
    protected abstract bool PlayerControllerIsValid(LocalPlayerController localPlayerController);

    private void OnTriggerEnter(Collider other)
    {
        LocalPlayerController localPlayerController = other.gameObject.GetComponentInParent<LocalPlayerController>();
        if (localPlayerController == null) { return; }
        if (!PlayerControllerIsValid(localPlayerController)) { return; }


        if (interactableIcon != null)
        {
            interactableIcon.OnPlayerEnterRange(validLocalPlayerController);
        }
        else
        {
            Debug.LogWarning("Interactable icon is null");
        }

        localPlayerController.validInteractables.Add(this);
        validLocalPlayerController = localPlayerController;
    }

    private void OnTriggerExit(Collider other)
    {
        LocalPlayerController localPlayerController = other.gameObject.GetComponentInParent<LocalPlayerController>();
        if (localPlayerController == null) { return; }
        if (!PlayerControllerIsValid(localPlayerController)) { return; }


        if (interactableIcon != null)
        {
            interactableIcon.OnPlayerExitRange(validLocalPlayerController);
        }
        else
        {
            Debug.LogWarning("Interactable icon is null");
        }

        if (localPlayerIsInteracting) { StopInteract(); }
        localPlayerController.validInteractables.Remove(this);
        validLocalPlayerController = null;
    }

    private void OnDestroy()
    {
        if (validLocalPlayerController != null)
        {
            validLocalPlayerController.validInteractables.Remove(this);
        }
    }
}

using UnityEngine;

public interface IInteractableIcon
{
    public void OnPlayerEnterRange(LocalPlayerController localPlayerController);

    public void OnPlayerExitRange(LocalPlayerController localPlayerController);

    public void OnInteract(LocalPlayerController localPlayerController);

    public void OnStopInteract(LocalPlayerController localPlayerController);
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerController : MonoBehaviour
{
    public List<Interactable> validInteractables;

    public NetworkedPlayerGameplayInfo NetworkedPlayerGameplayInfo { get; private set; }

    private bool interacting;
    private float lastInteractTime;
    private float interactCooldown = 0.7f;
    public float InteractCooldownNormalized
    {
        get
        {
            if (interacting)
            {
                return 0f;
            }
            else
            {
                return Mathf.Clamp((Time.time - lastInteractTime) / interactCooldown, 0f, 1f);
            }
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        validInteractables = new();
        GameSpawningManager.Instance.player = this.gameObject;
    }

    public void SetNetworkedPlayerGameplayInfo(NetworkedPlayerGameplayInfo networkedPlayerGameplayInfo)
    {
        NetworkedPlayerGameplayInfo = networkedPlayerGameplayInfo;
    }

    void OnInteract(InputValue inputValue)
    {
        if (validInteractables.Count == 0) { return; }

        if (inputValue.isPressed)
        {
            if (Time.time >= lastInteractTime + interactCooldown)
            {
                interacting = true;
                validInteractables[^1].Interact();
            }
        }
        else
        {
            interacting = false;
            lastInteractTime = Time.time;
            foreach (Interactable interactable in validInteractables)
            {
                if (interactable.LocalPlayerIsInteracting)
                {
                    interactable.StopInteract();
                }
            }
        }
    }

    void OnAttack(InputValue inputValue)
    {
        if (MainCanvasController.Instance is GameCanvasController gameCanvasController)
        {
            if (gameCanvasController.MenuIsShown) { return; }
        }
        else
        {
            Debug.LogWarning("LocalPlayerController could not find a GameCanvasController");
        }
        Cursor.lockState = CursorLockMode.Locked;
    }
    void OnEsc(InputValue inputValue)
    {
        Cursor.lockState = CursorLockMode.None;
        if (MainCanvasController.Instance is GameCanvasController canvas)
        {
            Debug.Log("OnEscInput being called from LocalPlayerController");
            canvas.OnEscInput();
        }
        else
        {
            Debug.LogWarning("Player could not find a valid GameCanvasController!");
        }
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

}

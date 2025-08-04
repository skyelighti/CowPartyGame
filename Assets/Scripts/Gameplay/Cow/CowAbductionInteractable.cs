using UnityEngine;

public class CowAbductionInteractable : Interactable
{
    [SerializeField] private CowController cowController;
    [SerializeField] private Transform cowTransform;

    public CowInteractableIcon CowInteractableIcon => (CowInteractableIcon)interactableIcon;

    private float interactStartTime;
    [SerializeField] private float interactTime;

    protected override void Start()
    {
        interactStartTime = float.MaxValue;
        base.Start();
    }

    protected override bool PlayerControllerIsValid(LocalPlayerController localPlayerController)
    {
        return localPlayerController.NetworkedPlayerGameplayInfo.isAlien && !cowController.FlaggedToAbduct;
    }

    public override void Interact()
    {
        if (interactStartTime == float.MaxValue)
        {
            interactStartTime = Time.time;
            base.Interact();
        }
    }

    public override void StopInteract()
    {
        if (interactStartTime != float.MaxValue)
        {
            interactStartTime = float.MaxValue;
            base.StopInteract();
        }
    }

    private void Update()
    {
        if (Time.time >= interactStartTime + interactTime)
        {
            cowController.SetAbductionFlagStatusRpc(true);
            interactStartTime = float.MaxValue;
            UfoManager.Instance.SummonUfoToAbductRpc(cowController.id.Value);
        }
    }
}

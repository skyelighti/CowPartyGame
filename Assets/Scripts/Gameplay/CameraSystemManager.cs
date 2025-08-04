using UnityEngine;
using Unity.Cinemachine;
using Unity.VisualScripting;
using Unity.Netcode;

public class CameraSystemManager : NetworkBehaviour
{
    public static CameraSystemManager Instance { get; private set; }
    private Camera cam;
    private CinemachineBrain brain;
    // private bool assignedDefaultCameraOffset;
    // private Vector3 defaultCameraOffsetDirection;
    [SerializeField] private CinemachineOrbitalFollow cinemachineOrbitalFollow;
    [SerializeField] private CinemachineRotationComposer cinemachineRotationComposer;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float maximumRadius;
    [SerializeField] private float minimumRadius = 1f;
    [SerializeField] private float radiusPadding = 1f;
    private Vector2 localActiveRotationComposerScreenPosition;

    [SerializeField] private FMODUnity.EventReference farMoo;

    public bool useRaycast;

    private void Awake()
    {
        localActiveRotationComposerScreenPosition = cinemachineRotationComposer.Composition.ScreenPosition;
        //Debug.Log($"localActiveRotationComposerScreenPosition: {localActiveRotationComposerScreenPosition}");
        Instance = this;
    }

    private void Start()
    {
        cam = Camera.main;
        brain = cam.GetComponent<CinemachineBrain>();
        cinemachineOrbitalFollow.Radius = maximumRadius;
        useRaycast = true;
        // Invoke(nameof(AssignDefaultCameraOffset), 0.5f);
    }

    /*
    private void AssignDefaultCameraOffset()
    {
        // assignedDefaultCameraOffset = true;
        // defaultCameraOffsetDirection = (brain.State.GetFinalPosition() - cinemachineCamera.Target.TrackingTarget.position).normalized;
    }
    */

    private void Update()
    {
        // if (!assignedDefaultCameraOffset) { return; }
        if (useRaycast)
        {
            Vector3 playerPos = cinemachineCamera.Target.TrackingTarget.position;

            Vector3 forward = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction;
            Vector3 dir1 = Quaternion.Euler(0, -21.9f, 0) * -forward;
            Vector3 dir2 = Quaternion.Euler(0, -14.586f, 0) * -forward;

            bool ray1Hit = Physics.Raycast(playerPos, dir1, out RaycastHit hit1, maximumRadius + radiusPadding);
            bool ray2Hit = Physics.Raycast(playerPos, dir2, out RaycastHit hit2, maximumRadius + radiusPadding);

            if (ray1Hit || ray2Hit)
            {
                float d1 = Vector3.Distance(playerPos, hit1.point);
                float d2 = Vector3.Distance(playerPos, hit2.point);
                cinemachineOrbitalFollow.Radius = Mathf.Max(minimumRadius, Mathf.Min(d1, d2) - radiusPadding);
                return;
            }
        }

        cinemachineOrbitalFollow.Radius = maximumRadius;
    }

    public void AssignFollowTarget(Transform transform, bool isLocal = true)
    {
        if (MainCanvasController.Instance is GameCanvasController gameCanvasController)
        {
            gameCanvasController.SetIsLocal(isLocal);
        }
        else
        {
            Debug.LogWarning("CameraSystemManager could not find a GameCanvasController");
        }

        if (isLocal)
        {
            //Debug.Log($"IsLocal. localActiveRotationComposerScreenPosition: {localActiveRotationComposerScreenPosition}");
            cinemachineRotationComposer.Composition.ScreenPosition = localActiveRotationComposerScreenPosition;
        }
        else
        {
            //Debug.Log($"Is not local");
            cinemachineRotationComposer.Composition.ScreenPosition = Vector2.zero;
        }

        cinemachineCamera.Target.TrackingTarget = transform;
    }

    [Rpc(SendTo.Everyone)]
    public void OnCowAbductedRpc()
    {
        if (MainCanvasController.Instance is GameCanvasController canvas)
        {
            canvas.FlashGreen(1.5f);
        }
        else
        {
            Debug.LogWarning("CameraSystemManager could not find a valid GameCanvasController");
        }
        FMODUnity.RuntimeManager.PlayOneShot(farMoo);
        GetComponentInChildren<CameraShake>().ShakeCameraLerp(7f, 1.5f);
    }
}

using UnityEngine;

public class MovingMenuCamFXHandler : CamFXHandler
{
    [SerializeField] private AnimationCurve travelAnimCurve;
    [SerializeField] private AnimationCurve rotationAnimCurve;

    [SerializeField] private float defaultMoveDuration;
    private float moveStartTime;
    private float currentMoveDuration;

    private Quaternion startAngle;
    private Quaternion endAngle;

    private Vector3 startPos;
    private Vector3 endPos;

    private void Start()
    {
        SetMoveTarget(transform.position, transform.rotation);
    }

    /// <summary>
    /// Sets the move target of the camera, which it will reach after defaultMoveDuration
    /// </summary>
    /// <param name="transform">The transform to sample the rotation and position of at the calling of this function</param>
    public void SetMoveTarget(Transform transform)
    {
        SetMoveTarget(transform.position, transform.rotation);
    }

    /// <summary>
    /// Sets the move target of the camera, which it will reach after defaultMoveDuration
    /// </summary>
    /// <param name="position">The position to reach after defaultMoveDuration, interpolated with travelAnimCurve</param>
    /// <param name="angle">The rotation to reach after defaultMoveDuration, interpolated with rotationAnimCurve</param>
    public void SetMoveTarget(Vector3 position, Quaternion angle)
    {
        SetMoveTarget(position, angle, defaultMoveDuration);
    }

    /// <summary>
    /// Sets the move target of the camera, which it will reach after defaultMoveDuration
    /// </summary>
    /// <param name="position">The position to reach after defaultMoveDuration, interpolated with travelAnimCurve</param>
    /// <param name="angle">The rotation to reach after defaultMoveDuration, interpolated with rotationAnimCurve</param>
    /// <param name="moveTime">A duration to use for this transition instead of defaultMoveDuration</param>
    public void SetMoveTarget(Vector3 position, Quaternion angle, float moveTime)
    {
        moveStartTime = Time.time;
        startPos = transform.position;
        startAngle = transform.rotation;
        endPos = position;
        endAngle = angle;

        currentMoveDuration = moveTime;
    }

    private void Update()
    {
        transform.SetPositionAndRotation(
            Vector3.LerpUnclamped(startPos, endPos, travelAnimCurve.Evaluate((Time.time - moveStartTime) / currentMoveDuration)),
            Quaternion.LerpUnclamped(startAngle, endAngle, rotationAnimCurve.Evaluate((Time.time - moveStartTime) / currentMoveDuration))
        );
    }
}

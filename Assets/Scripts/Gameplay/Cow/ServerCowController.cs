using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// The controller for cows that exists only on the server
/// </summary>
public class ServerCowController : MonoBehaviour
{
    #region Enums
    private enum CowState
    {
        Wander = 0,
        Wait = 1,
        Follow = 2,
        Abduct = 3,
        InUfo = 4,
        Falling = 5,
        Num
    }

    // A custom data type for reporting the result of an attempt to follow the NavMesh path.
    private enum WalkToTargetPointResult
    {
        Walking = 0, // Correctly following the path
        Arrived = 1, // Within distanceToTargetForChooseNewTarget of the targetPosition
        PathBlocked = 2, // An error occured in finding the path
        Num // The number of possible WalkToTargetPointResults
    }
    #endregion

    public CowController cowController;

    NavMeshPath navPath;

    // Used during Wandering
    Vector3 targetPosition;

    // Used during Abduction, InUfo, and Follow
    Transform targetTransform;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float distanceToTargetForChooseNewTarget = 2.5f;
    [SerializeField] private float maxTimeInSingleWander = 10f;
    [SerializeField] private float abductionSpeed = 1.2f;
    bool hasAddedOriginForce;

    private Rigidbody rb;
    private StateMachine stateMachine;

    private float noPathFoundStartTime;

    ServerUfoController abductingUfo;

    private Animator Anim;
    [SerializeField] private Bounds wanderBounds;

    void Start()
    {
        wanderBounds = CowManager.Instance.DefaultWanderBounds;
        navPath = new NavMeshPath();
        rb = gameObject.GetComponent<Rigidbody>();
        InitStateMachine();
        Anim = gameObject.GetComponentInChildren<Animator>();
    }

    void InitStateMachine()
    {
        stateMachine = new((int)CowState.Num);

        stateMachine.SetStateFunctions((int)CowState.Wander, EnterWanderState, UpdateWanderState, ExitWanderState);
        stateMachine.SetStateFunctions((int)CowState.Wait, EnterWaitState, UpdateWaitState, ExitWaitState);
        stateMachine.SetStateFunctions((int)CowState.Follow, EnterFollowState, UpdateFollowState, ExitFollowState);
        stateMachine.SetStateFunctions((int)CowState.Abduct, EnterAbductState, UpdateAbductState, ExitAbductState);
        stateMachine.SetStateFunctions((int)CowState.InUfo, EnterInUfoState, UpdateInUfoState, ExitInUfoState);
        stateMachine.SetStateFunctions((int)CowState.Falling, EnterFallingState, UpdateFallingState, ExitFallingState);

        stateMachine.SetState((int)CowState.Wander);
    }

    #region State machine functions
    void EnterWanderState()
    {
        ChooseNewRandomTarget();
        Anim.SetBool("Walking", true);

    }

    void UpdateWanderState()
    {
        WalkToTargetPointResult res = WalkToTargetPoint(targetPosition, false);
        if (res == WalkToTargetPointResult.Arrived) // Upon arrival, wait for a bit before continuing.
        {
            stateMachine.SetState((int)CowState.Wait);
        }
        else if (res == WalkToTargetPointResult.PathBlocked)
        {
            // noPathFoundStartTime will be greater than the current time if there was previously a path found.
            // Once there is again a path found, noPathFoundStartTime is set to float.MaxValue
            if (noPathFoundStartTime > Time.time)
            {
                noPathFoundStartTime = Time.time;
            }
            if (Time.time >= noPathFoundStartTime + CowManager.Instance.noPathFoundAllowedDuration)
            {
                ChooseNewRandomTarget();
            }
        }
        else
        {
            // This else block is executed when the cow is still Walking.
            noPathFoundStartTime = float.MaxValue;
        }
        
        if (stateMachine.TimeInState >= maxTimeInSingleWander)
        {
            stateMachine.SetState((int)CowState.Wait);
        }
    }

    void ExitWanderState()
    {

    }

    void EnterWaitState()
    {
        if (Anim != null)
        {
            Debug.Log("Anim setting walking false");
            Anim.SetBool("Walking", false);
        }

    }

    void UpdateWaitState()
    {
        // stateMachine.TimeInState reports how long the state machine has been in its current state
        if (stateMachine.TimeInState > CowManager.Instance.waitTime)
        {
            stateMachine.SetState((int)CowState.Wander);
        }
    }

    void ExitWaitState()
    {

    }

    void EnterFollowState()
    {
        Anim.SetBool("Walking", true);
    }

    void UpdateFollowState()
    {
        WalkToTargetPoint(targetTransform.position, true);
    }

    void ExitFollowState()
    {

    }

    void EnterAbductState()
    {
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ |
            RigidbodyConstraints.FreezePositionX |
            RigidbodyConstraints.FreezePositionZ;
        if (Anim != null)
        {
            Debug.Log("Anim setting walking false");
            Anim.SetBool("Walking", false);
        }
    }

    void UpdateAbductState()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            abductionSpeed * Time.deltaTime
        );

        transform.localScale = Vector3.MoveTowards(
            transform.localScale,
            new Vector3(0.1f, 0.1f, 0.2f),
            abductionSpeed * Time.deltaTime
        );
        
        if (Mathf.Abs(transform.position.y - targetPosition.y) < 0.3f) // If you're close enough to the ufo
        {
            abductingUfo.OnAbductionComplete();
            stateMachine.SetState((int)CowState.InUfo);
        }
    }

    void ExitAbductState()
    {
        rb.useGravity = true;
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;
        transform.localScale = new Vector3(1, 1, 2);
        cowController.SetAbductionFlagStatusRpc(false);
    }

    void EnterInUfoState()
    {
        rb.useGravity = false;
        cowController.DisableVisualsRpc();
    }

    void UpdateInUfoState()
    {
        targetPosition = targetTransform.position;
        transform.position = targetPosition;
    }

    void ExitInUfoState()
    {
        rb.useGravity = true;
        cowController.EnableVisualsRpc();
    }

    void EnterFallingState()
    {
        cowController.SetBouncyRpc(true);
        hasAddedOriginForce = false;
    }

    void UpdateFallingState()
    {
        float maxSampleDistance =
            stateMachine.TimeInState >= CowManager.Instance.falingToGroundStateMaxTime ?
            100f : // An arbitrarily large number for a wide search
            CowManager.Instance.distanceToGroundToStopFalling;

        bool res = NavMesh.SamplePosition(
            transform.position,
            out NavMeshHit hit,
            maxSampleDistance,
            NavMesh.AllAreas
        );

        if (res)
        {
            transform.position = hit.position;
            stateMachine.SetState((int)CowState.Wander);
        }
    }

    void ExitFallingState()
    {
        wanderBounds = CowManager.Instance.GetNewWanderBoundsAtPos(transform.position);
        cowController.SetBouncyRpc(false);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (stateMachine.CurrentState != (int)CowState.Falling) { return; }
        if (hasAddedOriginForce) { return; }

        rb.AddForce(
            (CowManager.Instance.CowOrigin - transform.position).normalized *
            CowManager.Instance.toCowOriginForce,
            ForceMode.Impulse
        );
    }
    #endregion

    private WalkToTargetPointResult WalkToTargetPoint(Vector3 position, bool sampleIfBlocked)
    {
        if (!NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, navPath) || navPath.corners.Length <= 1)
        {
            if (sampleIfBlocked)
            {
                NavMesh.SamplePosition(position, out NavMeshHit hit, 50f, NavMesh.AllAreas);
                Vector3 sampledPos = hit.position;
                return WalkToTargetPoint(sampledPos, false);
            }

            return WalkToTargetPointResult.PathBlocked;
        }

        if (navPath.corners.Length == 2 && Vector3.Distance(navPath.corners[1], navPath.corners[0]) < distanceToTargetForChooseNewTarget)
        {
            return WalkToTargetPointResult.Arrived;
        }

        Vector3 curTravelVector = navPath.corners[1] - navPath.corners[0];

        rb.linearVelocity = curTravelVector.normalized * moveSpeed;
        transform.LookAt(navPath.corners[1]);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        return WalkToTargetPointResult.Walking;
    }

    void Update()
    {
        stateMachine.UpdateState();
        if (transform.position.y < -20f)
        {
            transform.position = CowManager.Instance.CowOrigin + Vector3.up * 20f;
            rb.linearVelocity = Vector3.zero;
            stateMachine.SetState((int)CowState.Falling);
        }
    }

    private void ChooseNewRandomTarget()
    {
        targetPosition = CowManager.Instance.GetRandomNavMeshPointInBounds(wanderBounds);
        if (Anim != null)
        {
            Debug.Log("Anim setting walking true");
            //Anim.SetBool("Walking",true);
        }
    }

    public void StartAbduction(ServerUfoController abductingUfo)
    {
        stateMachine.SetState((int)CowState.Abduct);
        this.abductingUfo = abductingUfo;
        targetTransform = abductingUfo.transform;
        targetPosition = targetTransform.position;
    }

    public void StartFollowing(Transform target)
    {
        if (stateMachine.CurrentState == (int)CowState.Abduct) { return; }
        if (stateMachine.CurrentState == (int)CowState.InUfo) { return; }

        targetTransform = target;
        stateMachine.SetState((int)CowState.Follow);
    }

    public void StopFollowing(Transform target)
    {
        if (targetTransform == target)
        {
            targetTransform = null;
            stateMachine.SetState((int)CowState.Wander);
        }
    }

    public void FailAbduction()
    {
        stateMachine.SetState((int)CowState.Falling);
    }
}

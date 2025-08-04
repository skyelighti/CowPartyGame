using Unity.Netcode;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

public class ServerUfoController : NetworkBehaviour, IDamageable
{
    private enum UfoState
    {
        Chase = 0,
        Abduct = 1,
        Return = 2,
        //Wait = 3,
        Num
    }

    private StateMachine stateMachine;

    public ulong id;

    public Spline spline = null;
    private AnimationCurve curve;

    [SerializeField] private float chaseSpeed;
    [SerializeField] private float returnSpeed;
    private float speed;
    private float timeToReachTarget;
    public Vector3 origin;
    public float traverseY;
    public float hoverHeight;
    public ulong targetCowId;
    public Transform targetTransform;
    public ServerCowController targetCow;

    [SerializeField] private GameObject CowAbductVFX;
    [SerializeField] private GameObject OnDeathVFX;

    private NetworkObject networkObject;

    private float furthestT;
    private bool abductionCompleted;

    [SerializeField] private int health = 2;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            Destroy(this);
            return;
        }
        ReworkSpline();
        MoveToSplinePoint(0, false, false);
    }

    private void Start()
    {
        networkObject = GetComponent<NetworkObject>();
        InitStateMachine();
    }

    private void InitStateMachine()
    {
        stateMachine = new((int)UfoState.Num);

        stateMachine.SetStateFunctions((int)UfoState.Chase, EnterChaseState, UpdateChaseState, ExitChaseState);
        stateMachine.SetStateFunctions((int)UfoState.Abduct, EnterAbductState, UpdateAbductState, ExitAbductState);
        stateMachine.SetStateFunctions((int)UfoState.Return, EnterReturnState, UpdateReturnState, ExitReturnState);
        //stateMachine.SetStateFunctions((int)UfoState.Wait, EnterWaitState, UpdateWaitState, ExitWaitState);

        stateMachine.SetState((int)UfoState.Chase);
    }


    private void EnterChaseState()
    {
        curve = UfoManager.Instance.chaseAnimationCurve;
        speed = chaseSpeed;
    }

    private void UpdateChaseState()
    {
        ReworkSpline();
        furthestT = stateMachine.TimeInState / timeToReachTarget;
        MoveToSplinePoint(furthestT, true, false);
    }

    private void ExitChaseState()
    {

    }

    private void EnterAbductState()
    {
        // timeToReachTarget is timeToReachTarget = Vector3.Distance(point3, point2) / speed, so 
        // Vector3.Distance(point3, point2) can be found by multiplying by speed. Then dividing by
        // the new speed sets the timeToReachTarget correctly.
        timeToReachTarget *= speed;
        speed = returnSpeed;
        timeToReachTarget /= speed;
        CowAbductRpc();
        //Add VFX here

        //sfx
        FMODUnity.RuntimeManager.PlayOneShot("event:/UFO_Beam", transform.position);

        curve = UfoManager.Instance.returnAnimationCurve;
    }

    private void UpdateAbductState()
    {

    }

    private void ExitAbductState()
    {

    }

    private void EnterReturnState()
    {

    }

    private void UpdateReturnState()
    {
        MoveToSplinePoint(furthestT - (stateMachine.TimeInState / timeToReachTarget), false, true);
    }

    private void ExitReturnState()
    {

    }

    /*
    private void EnterWaitState()
    {

    }

    private void UpdateWaitState()
    {

    }

    private void ExitWaitState()
    {

    }
    */

    private void Update()
    {
        stateMachine.UpdateState();
    }

    public void ReworkSpline()
    {
        Vector3 point2 = origin;
        point2.y = traverseY;
        Vector3 point3 = targetTransform.position;
        point3.y = traverseY;
        Vector3 point4 = targetTransform.position;
        point4.y += hoverHeight;
        spline = new(
            new List<float3>()
            {
                origin,
                point2,
                point3,
                point4
            }
        );
        timeToReachTarget = Vector3.Distance(point3, point2) / speed;
    }

    private void MoveToSplinePoint(float t, bool abductAt1, bool despawnAt0)
    {
        if (spline == null)
        {
            Debug.LogWarning("Spline is null!");
            return;
        }
        SplineUtility.Evaluate(spline,
            curve.Evaluate(t),
            out float3 position, out float3 tangent, out float3 upVector
        );
        transform.localPosition = position;
        if (abductAt1 && t >= 1)
        {
            targetCow.StartAbduction(this);
            stateMachine.SetState((int)UfoState.Abduct);
        }
        else if (despawnAt0 && t <= 0)
        {
            if (abductionCompleted)
            {
                CowManager.Instance.CowInMothership(targetCowId);
                CameraSystemManager.Instance.OnCowAbductedRpc();
            }
            UfoManager.Instance.DespawnUfo(networkObject, targetCowId);
        }
    }

    public void OnAbductionComplete()
    {
        stateMachine.SetState((int)UfoState.Return);
        abductionCompleted = true;
    }

    public void OnStopCalling()
    {
        if (stateMachine.CurrentState == (int)UfoState.Chase)
        {
            stateMachine.SetState((int)UfoState.Return);
        }
    }

    public bool OnHit(ulong sourcePlayerId, int damage)
    {
        Debug.Log($"OnHit called, {damage}");
        DamageRpc();
        health -= damage;
        if (health <= 0)
        {
            DestroyRpc();

            //Spawn VFX
            CowManager.Instance.FreeCow(targetCowId);
            UfoManager.Instance.DespawnUfo(networkObject, targetCowId);
            return true;
        }
        return false;
    }

    [Rpc(SendTo.Everyone)]
    public void CowAbductRpc()
    {
        Debug.Log("SHould be sent to everyone");
        var trail = Instantiate(CowAbductVFX, targetCow.gameObject.transform.position, Quaternion.identity);
        var trailnet = trail.GetComponent<NetworkObject>();
        if (trailnet != null)
        {
            trailnet.Spawn();
        }
    }
    [Rpc(SendTo.Everyone)]
    public void DestroyRpc()
    {
        //print("Destroyed");
        Camera.main.transform.parent.gameObject.GetComponentInChildren<CameraShake>().ShakeCameraLerp(10f, 1.2f);
        var trail = Instantiate(OnDeathVFX, transform.position, Quaternion.identity);
        var trailnet = trail.GetComponent<NetworkObject>();
        if (trailnet != null)
        {
            trailnet.Spawn();
        }


    }
    
    [Rpc(SendTo.Everyone)]
    public void DamageRpc()
    {
        print("damaged ufo");
        //Camera.main.transform.parent.gameObject.GetComponentInChildren<CameraShake>().ShakeCameraLerp(10f, 1.2f);
        var trail = Instantiate(OnDeathVFX, transform.position, Quaternion.identity);
        var trailnet = trail.GetComponent<NetworkObject>();
        if (trailnet != null)
        {
            trailnet.Spawn();
        }


    }
}

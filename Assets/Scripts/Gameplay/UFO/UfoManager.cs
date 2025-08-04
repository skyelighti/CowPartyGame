using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Splines;

public class UfoManager : NetworkBehaviour
{
    public static UfoManager Instance { get; private set; }

    [SerializeField] private Transform mothership;
    [SerializeField] private GameObject Ufo;
    [SerializeField] private Transform mothershipTransform;
    [SerializeField] private float traverseY;
    [SerializeField] private float hoverHeight;

    public AnimationCurve chaseAnimationCurve;
    public AnimationCurve returnAnimationCurve;

    [SerializeField] private float mothershipRotateSpeed = 1;

    private ulong nextId;

    // Only used on the server
    private Dictionary<ulong, ServerUfoController> cowIdToServerUfo = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        mothership.transform.rotation =Quaternion.Euler(
            0,
            mothership.transform.rotation.eulerAngles.y + mothershipRotateSpeed * Time.deltaTime,
            0
        );
    }

    [Rpc(SendTo.Server)]
    public void SummonUfoToAbductRpc(ulong targetId)
    {
        if (cowIdToServerUfo.ContainsKey(targetId))
        {
            Debug.Log("Cow is already being abducted!");
            return;
        }

        GameObject NewUfo = Instantiate(Ufo, transform.position, Quaternion.identity);

        NetworkObject networkObject = NewUfo.GetComponent<NetworkObject>();

        ServerUfoController serverUfoController = NewUfo.GetComponent<ServerUfoController>();
        serverUfoController.id = nextId;
        nextId++;
        cowIdToServerUfo[targetId] = serverUfoController;

        AssignSpline(serverUfoController, targetId);
        networkObject.Spawn();
    }

    [Rpc(SendTo.Server)]
    public void EarlyRetreatUfoRpc(ulong targetId)
    {
        if (!cowIdToServerUfo.ContainsKey(targetId))
        {
            Debug.Log("UFO to retreat does not exist!");
            return;
        }

        cowIdToServerUfo[targetId].OnStopCalling();
        cowIdToServerUfo.Remove(targetId);
    }

    public void DespawnUfo(NetworkObject net, ulong targetId)
    {
        cowIdToServerUfo.Remove(targetId);
        net.Despawn();
    }

    private void AssignSpline(ServerUfoController serverUfoController, ulong targetId)
    {
        Spline spline = new();
        serverUfoController.origin = mothershipTransform.position;
        serverUfoController.traverseY = traverseY;
        serverUfoController.hoverHeight = hoverHeight;
        serverUfoController.targetCowId = targetId;
        serverUfoController.targetTransform = CowManager.Instance.cows[targetId].transform;
        serverUfoController.targetCow = CowManager.Instance.cows[targetId].GetComponent<ServerCowController>();
        serverUfoController.spline = spline;
    }
}

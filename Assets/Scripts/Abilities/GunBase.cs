using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public abstract class GunBase : PlayerAbilties
{
    //[SerializeField] private GameObject sphere;
    [SerializeField] private int damage;
    public RaycastHit hit;
    public GameObject BulletTrail;
    public GameObject ImpactSystem;
    //private Transform raycastMarker;
    Camera cam;
    CinemachineBrain brain;
    LayerMask layerMask;
    bool recentAtk = false;
    bool enteringAttacking = false;

    private new void OnEnable()
    {
        base.OnEnable();
        // raycastMarker = Instantiate(sphere).transform;
        cam = Camera.main;
        brain = Camera.main.GetComponent<CinemachineBrain>();
        layerMask = LayerMask.GetMask("Default", "Player", "UFO");
    }

    public override void Ability()
    {
        if (IsOwner)
        {
            NetSfx.Play(SfxId.abilities, transform.position, 1);//shoot sound
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 camPos = brain.State.GetFinalPosition();
            Vector3 toPlayer = transform.position - camPos;
            Vector3 forward = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction;
            ray.origin = camPos + Vector3.Project(toPlayer, forward);
            var netAnim = gameObject.GetComponent<NetAnimManager>();
            if (netAnim != null)
            {
                if (netAnim.isAttacking.Value == false)
                {
                    netAnim.isAttacking.Value = true;
                    Debug.Log(netAnim.isAttacking.Value);
                    StartCoroutine(StartShootingCheck());
                }
                else if (!netAnim.isShooting.Value && !enteringAttacking)
                {
                    enteringAttacking = true;
                    netAnim.isShooting.Value = true;
                }
                //Finish turn off anim in a bit

            }
            ulong localClienId = NetworkManager.Singleton.LocalClientId;
            ShootServerServerRpc(
                localClienId,
                gameObject.tag,
                ray.origin,
                ray.direction
            );
            recentAtk = true;

            cam.transform.parent.gameObject.GetComponentInChildren<CameraShake>().ShakeCameraLerp(2f, 0.2f);
        }
    }

    IEnumerator StartShootingCheck()
    {
        yield return new WaitForSeconds(0.5f);
        var netAnim = gameObject.GetComponent<NetAnimManager>();
        while (netAnim.isAttacking.Value)
        {
            if (!recentAtk)
            {
                if (enteringAttacking)
                {
                    netAnim.isShooting.Value = false;
                    enteringAttacking = false;
                }
                else
                {
                    netAnim.isAttacking.Value = false;
                }
            }
            recentAtk = false;
        }
    }

    [Rpc(SendTo.Server)]
    public void ShootServerServerRpc(ulong sourceId, string shooterTag, Vector3 origin, Vector3 direction)
    {
        //maybe add a spread to the gun
        Ray ray = new(origin, direction);
        if (Physics.Raycast(ray, out hit, layerMask))
        {
            if (!hit.transform.gameObject.CompareTag(shooterTag))
            {
                NetSfx.Play(SfxId.hurt, hit.point);//hurt sound
                
                if (hit.transform.gameObject.TryGetComponent(out IDamageable damageable))
                {
                    if (damageable.OnHit(sourceId, damage))
                    {
                        InformShooterOfKillRpc(NetworkManager.Singleton.RpcTarget.Single(sourceId, RpcTargetUse.Temp));
                    }
                    ServerGunOnHit();
                }
            }
            Vector3 ShootPos = gameObject.GetComponentInChildren<SphereCollider>().transform.position;
            var trail = Instantiate(BulletTrail, ShootPos, Quaternion.identity);
            var trailRenderer = trail.GetComponent<TrailRenderer>();
            var trailnet = trail.GetComponent<NetworkObject>();
            trailnet.Spawn();
            StartCoroutine(SpawnTrail(trailRenderer, hit.point, true, hit.normal));
        }
        else
        {
            var trail = Instantiate(BulletTrail, transform.position, Quaternion.identity);
            var trailRenderer = trail.GetComponent<TrailRenderer>();
            var trailnet = trail.GetComponent<NetworkObject>();
            trailnet.Spawn();
            StartCoroutine(SpawnTrail(trailRenderer, origin + direction * 100f, false, Vector3.zero));
        }
    }

    IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 target, bool showImpact, Vector3 normal)
    {
        float time = 0;
        Vector3 startPosition = Trail.transform.position;

        while (time < 0.1)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, target, time);
            time += Time.deltaTime / Trail.time;

            yield return null;
        }
        Trail.transform.position = target;

        if (showImpact)
        {
            var impact = Instantiate(ImpactSystem, target, Quaternion.LookRotation(normal));
            var impactnet = impact.GetComponent<NetworkObject>();
            impactnet.Spawn();
        }

        Destroy(Trail.gameObject, Trail.time);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void InformShooterOfKillRpc(RpcParams rpcParams)//kc
    {
        if (Random.value >= (1f / 3f))//play 1/3 of the time
            return;
        Vector3 pos = Vector3.zero;
        var localPlayerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayerObj != null) pos = localPlayerObj.transform.position;
        else if (Camera.main != null) pos = Camera.main.transform.position;
        string[] pool = new string[]
        {
            "Event:/Take It",
            "Event:/Walking Around",
            "Event:/Nice Shooting",
            "Event:/Comeon Now",
            "Event:/Yer Kind",
        };//get player position logic
        var chosen = pool[UnityEngine.Random.Range(0, pool.Length)];
        FMODUnity.RuntimeManager.PlayOneShot(pool[UnityEngine.Random.Range(0, pool.Length)], pos);//choose and play sound
    }
    public void Start()
    {
        //InformShooterOfKillRpc(new RpcParams { Send = NetworkManager.Singleton.RpcTarget.Me });//just to init the method so it works later
        Debug.Log("GunBase Inited");//testing informshooter seems to work
    }

    public abstract void ServerGunOnHit();

    public PlayerController player;
}

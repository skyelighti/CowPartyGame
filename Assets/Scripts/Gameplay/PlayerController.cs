using System.Threading;
using System;
using System.Collections.Generic; 
using System.Collections;
using System.Runtime.CompilerServices;
using Fragsurf.Movement;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

public class PlayerController : NetworkBehaviour, IDamageable
{
    public int maxHealth;

    public NetworkVariable<int> Health { get; } = new NetworkVariable<int>(5);
    private NetworkObject networkObject;
    private PlayerInput playerInput;
    private SurfCharacter surfChar;

    // Abilities MUST have all alien abilities to the left of FarmerShoot
    // FarmerShoot MUST be the first farmer ability
    private enum enability { StunGun, Dash, TransformPlayer, FarmerShoot, Num };
    /*
    [SerializeField] private NetworkVariable<enability> ability = new(enability.Num,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    */
    private Vector3 respawnpos;
    public float deathDelay;

    public NetworkedPlayerGameplayInfo NetworkedPlayerGameplayInfo { get; private set; }

    public bool stunned; // Only set on the local client
    private float unstunTime; // Only set on the local client

    private ulong idOfLastDamager;

    public override void OnNetworkSpawn()
    {
        networkObject = GetComponent<NetworkObject>();
        NetworkedPlayerGameplayInfo = NetworkedGameplayManager.Instance.GetNetworkedPlayerGameplayInfoOfId(networkObject.OwnerClientId);
        playerInput = transform.GetComponent<PlayerInput>();
        surfChar = transform.GetComponent<SurfCharacter>();
        /*
        if (ability.Value == enability.Num)
        {
            // If a non-owner runs this code after the owner has already set the ability, catch up.
            Debug.Log("Corrected a race condition!");
            SetAbility(enability.Num, ability.Value);
        }
        ability.OnValueChanged += SetAbility;
        */
        maxHealth = Health.Value;
        if (networkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            //if you are owner
            LocalPlayerController localPlayerController = gameObject.AddComponent<LocalPlayerController>();
            localPlayerController.SetNetworkedPlayerGameplayInfo(NetworkedPlayerGameplayInfo);
            CameraSystemManager.Instance.AssignFollowTarget(transform);
            if (MainCanvasController.Instance is GameCanvasController gameCanvasController)
            {
                gameCanvasController.playerController = this;
                gameCanvasController.localPlayerController = localPlayerController;
                gameCanvasController.playerObject = gameObject;
                // ((GameCanvasController)MainCanvasController.Instance).UpdateHealthSlider();
            }
        }
        else
        {
            //if youre not the owner of this
            Destroy(playerInput);
            // Apparently, destroying the surfChar now results in the collider not being created.
            // surfChar.CreateCollider();
            // Destroy(surfChar);
        }

        if (NetworkedPlayerGameplayInfo.isAlien)
        {
            if (IsOwner)
            {
                enability e = (enability)UnityEngine.Random.Range(0, (int)enability.TransformPlayer);
                SetAbilityRpc(true, e);
            }
            respawnpos = GameSpawningManager.Instance.AlienSpawns[0].transform.position;
            gameObject.tag = "Alien";
        }
        else
        {
            if (IsOwner)
            {
                enability e = (enability)UnityEngine.Random.Range((int)enability.FarmerShoot, (int)enability.Num);
                SetAbilityRpc(false, e);
            }
            respawnpos = GameSpawningManager.Instance.FarmerSpawn.transform.position;
            gameObject.tag = "Farmer";
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SetAbilityRpc(bool isAlien, enability ability)
    {
        switch (ability)
        {
            case enability.StunGun:
                gameObject.GetComponent<StunGun>().enabled = true;
                break;
            case enability.Dash:
                gameObject.GetComponent<Dash>().enabled = true;
                break;
            case enability.TransformPlayer:
                gameObject.GetComponent<TransformPlayer>().enabled = true;
                break;
            case enability.FarmerShoot:
                gameObject.GetComponent<FarmerShoot>().enabled = true;
                break;
            default:
                Debug.LogWarning("Attempting to set the ability to an invalid ability");
                break;
        }

        if (isAlien != NetworkedPlayerGameplayInfo.isAlien)
        {
            Debug.LogWarning($"Alien status mismatch.\nOwned by ID {networkObject.OwnerClientId}\nExecuted on {NetworkManager.Singleton.LocalClientId}\nParam: {isAlien}\nInfo: {NetworkedPlayerGameplayInfo.name}, {NetworkedPlayerGameplayInfo.id}, {NetworkedPlayerGameplayInfo.isAlien}");
        }
        
        if (isAlien)
        {
            GameObject obj = Instantiate(GameSpawningManager.Instance.alienMeshPrefab[(int)ability], transform);
            obj.transform.localPosition = Vector3.zero;
        }
        else
        {
            GameObject obj = Instantiate(GameSpawningManager.Instance.farmerMeshPrefab[(int)ability - (int)enability.FarmerShoot], transform);
            obj.transform.localPosition = Vector3.zero;
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void PlayerDieRpc(ulong sourceId, RpcParams rpcParams) // Only executed on the local client
    {
        idOfLastDamager = sourceId;
        StartCoroutine(LocalPlayerDieCoroutine());
    }

    private IEnumerator LocalPlayerDieCoroutine()
    {
        Vector3 respawnPos = NetworkedPlayerGameplayInfo.isAlien ? GetAlienRespawnPosition() : respawnpos;
        transform.position = respawnPos + Vector3.down * 5f;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        SurfCharacter surfCharacter = GetComponent<SurfCharacter>();
        surfCharacter.enabled = false;
        PlayerController damager = NetworkedGameplayManager.Instance.GetPlayerControllerOfId(idOfLastDamager);
        if (damager == null)
        {
            Debug.LogWarning("Could not find damager!");
            CameraSystemManager.Instance.AssignFollowTarget(null, false);
        }
        else
        {
            CameraSystemManager.Instance.AssignFollowTarget(damager.transform, false);
        }

        ResetHealthRpc();

        // Slightly longer so that Unstun can still end it early, thus ensuring that the respawn and unstun occur at the same time.
        LocalStartStun(deathDelay * 1.1f);
        yield return new WaitForSeconds(deathDelay);

        transform.position = respawnPos;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        surfCharacter.enabled = true;
        Unstun();
        CameraSystemManager.Instance.AssignFollowTarget(transform, true);
    }

    [Rpc(SendTo.Server)]
    private void ResetHealthRpc()
    {
        Health.Value = maxHealth;
    }

    private void Update()
    {
        if (stunned && Time.time >= unstunTime)
        {
            Unstun();
        }
    }

    private Vector3 GetAlienRespawnPosition()
    {
        Vector3 averageCowPos = CowManager.Instance.GetAverageCowPos();
        float lowestDistance = float.MaxValue;
        bool anyFound = false;
        bool posFound = false;
        Vector3 closestPos = Vector3.zero;
        foreach (GameObject spawn in GameSpawningManager.Instance.AlienSpawns)
        {
            Vector3 pos = spawn.transform.position;
            float dis = Vector3.Distance(averageCowPos, pos);
            anyFound = true;
            if (dis <= lowestDistance)
            {
                lowestDistance = dis;
                posFound = true;
                closestPos = pos;
            }
        }
        if (!anyFound)
        {
            throw new InvalidOperationException($"No spawn position at all found in GameSpawningManager.Instance.AlienSpawns. Length: {GameSpawningManager.Instance.AlienSpawns.Length}");
        }
        if (!posFound)
        {
            throw new InvalidOperationException($"No lowest spawn position found in GameSpawningManager.Instance.AlienSpawns. Length: {GameSpawningManager.Instance.AlienSpawns.Length}");
        }
        return closestPos;
    }

    public bool OnHit(ulong sourcePlayerId, int damage) // Only executed on the server
    {
        TakeDamageServerRpc(damage);
        if (Health.Value <= 0)
        {
            PlayerDieRpc(sourcePlayerId, NetworkManager.Singleton.RpcTarget.Single(networkObject.OwnerClientId, RpcTargetUse.Temp));
            return true;
        }
        return false;
    }

    SkinnedMeshRenderer[] mesh;
    List<Color> origColors = new List<Color>();
    Material mats;
    [Rpc(SendTo.Everyone)]
    void DamageFlashRpc()
    {
        mesh = GetComponentsInChildren<SkinnedMeshRenderer>();
        Debug.Log("Should Flash");
        if (mesh != null)
        {
            Debug.Log("Should Flash");
            foreach (SkinnedMeshRenderer m in mesh)
            {
                mats = m.material;
                //Color col = m.material.color;
                //origColors.Add(col);
                mats.SetColor("_Tint", Color.red);
            }
            StartCoroutine(ResetColors());
        }
    }
    IEnumerator ResetColors()
    {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < mesh.Length; i++)
            mesh[i].material.SetColor("_Tint", new Color(1f, 1f, 1f));
    }


    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        DamageFlashRpc();
        Health.Value -= damage;

    }

    // Called from the server. Specifically, it is called in StunGun.ServerGunOnHit()
    public void StartStun(float duration)
    {
        StartStunRpc(duration, NetworkedPlayerGameplayInfo.BaseRpcTarget);
    }

    // Do NOT make this public. Just call StartStun.
    [Rpc(SendTo.SpecifiedInParams)]
    private void StartStunRpc(float duration, RpcParams rpcParams) // Only executed on the local client
    {
        LocalStartStun(duration);
    }

    // Do NOT make this public. Just call StartStun.
    private void LocalStartStun(float duration) // Only executed on the local client
    {
        stunned = true;
        surfChar.ignorePlayerInput = true;
        unstunTime = Time.time + duration;
    }

    private void Unstun() // Only executed on the local client
    {
        stunned = false;
        surfChar.ignorePlayerInput = false;
    }
}

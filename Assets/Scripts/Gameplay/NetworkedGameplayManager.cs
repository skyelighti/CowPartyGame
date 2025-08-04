using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class NetworkedPlayerGameplayInfo : INetworkSerializable
{
    public bool isHost;
    public ulong id;
    public string name;
    public bool isAlien;
    public BaseRpcTarget BaseRpcTarget => NetworkManager.Singleton.RpcTarget.Single(id, RpcTargetUse.Temp);

    // IMPORTANT: If you add a field in NetworkedPlayerGameplayInfo that needs to be transferred across the network,
    // it must be serialized here in the magic black box function.
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref isHost);
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref isAlien);
    }

    // IMPORTANT: If you add a field that needs to remain in existence after the NetworkedPlayerGameplayInfo is converted into
    // a NetworkedPlayerLobbyInfo, it must be converted here.
    public static explicit operator NetworkedPlayerLobbyInfo(NetworkedPlayerGameplayInfo networkedPlayerGameplayInfo)
    {
        return new()
        {
            isHost = networkedPlayerGameplayInfo.isHost,
            id = networkedPlayerGameplayInfo.id,
            name = networkedPlayerGameplayInfo.name
        };
    }
}

public class NetworkedGameplayManager : NetworkBehaviour
{
    public static NetworkedGameplayManager Instance;

    public List<NetworkedPlayerGameplayInfo> NetworkedPlayerGameplayInfos { get; private set; }
    public NetworkedPlayerGameplayInfo LocalNetworkedPlayerGameplayInfo
    {
        get
        {
            foreach (NetworkedPlayerGameplayInfo net in NetworkedPlayerGameplayInfos)
            {
                if (net.id == NetworkManager.Singleton.LocalClientId)
                {
                    return net;
                }
            }
            throw new InvalidOperationException("A client attempted to get their LocalNetworkedPlayerGameplayInfo, but it does not exist.");
        }
    }

    public int CowsRemaining => CowManager.Instance.cowsRemaining;
    public int CowsAtGameStart => CowManager.Instance.cowsAtGameStart;
    public int WinconditionPercentageCows => CowManager.Instance.winconditionPercentageCows;

    [SerializeField] private GameObject RoundEndManagerPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (IsServer)
        {
            SetNetworkedPlayerGameplayInfos();
            AssignTeams();
            CowManager.Instance.SpawnCows(NetworkedPlayerGameplayInfos.Count * 3);
            InformNonServerOfNetworkedPlayerGameplayInfosRpc(NetworkedPlayerGameplayInfos.ToArray());
            GameSpawningManager.Instance.SpawnPlayers(NetworkedPlayerGameplayInfos);
        }
    }

    private void SetNetworkedPlayerGameplayInfos()
    {
        NetworkedPlayerGameplayInfos = new();
        foreach (NetworkedPlayerLobbyInfo networkedPlayerLobbyInfo in LobbyManager.Instance.NetworkedPlayerLobbyInfos)
        {
            NetworkedPlayerGameplayInfos.Add((NetworkedPlayerGameplayInfo)networkedPlayerLobbyInfo);
        }
    }

    private void AssignTeams()
    {
        List<NetworkedPlayerGameplayInfo> teamAssignmentList = new(NetworkedPlayerGameplayInfos);
        int c = teamAssignmentList.Count / 2;
        for (int i = 0; i < c; i++)
        {
            int j = UnityEngine.Random.Range(0, teamAssignmentList.Count);
            teamAssignmentList[j].isAlien = true;
            teamAssignmentList.RemoveAt(j);
        }
    }

    [Rpc(SendTo.NotServer)]
    private void InformNonServerOfNetworkedPlayerGameplayInfosRpc(NetworkedPlayerGameplayInfo[] networkedPlayerGameplayInfos)
    {
        NetworkedPlayerGameplayInfos = networkedPlayerGameplayInfos.ToList();
    }

    public NetworkedPlayerGameplayInfo GetNetworkedPlayerGameplayInfoOfId(ulong id)
    {
        foreach (NetworkedPlayerGameplayInfo net in NetworkedPlayerGameplayInfos)
        {
            if (net.id == id)
            {
                return net;
            }
        }
        throw new InvalidOperationException("A client attempted to get their LocalNetworkedPlayerGameplayInfo, but it does not exist.");
    }

    public PlayerController GetPlayerControllerOfId(ulong id)
    {
        foreach (PlayerController playerController in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (playerController.NetworkedPlayerGameplayInfo.id == id)
            {
                return playerController;
            }
        }
        return null;
    }

    // Only executed on the server
    public void OnCowInMothership()
    {
        if (((CowsAtGameStart-CowsRemaining)/((int)(CowsAtGameStart*(float)(WinconditionPercentageCows/100f))))>=1f)
        {
            EndRound(true);
        }
    }

    public void OnTimerZero()
    {
        EndRound(false);
    }

    private void EndRound(bool aliensWon)
    {
        GameObject obj = Instantiate(RoundEndManagerPrefab);
        RoundEndManager roundEndManager = obj.GetComponent<RoundEndManager>();
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        networkObject.Spawn();
        roundEndManager.ConfigureRoundEndRpc(aliensWon);
    }
}

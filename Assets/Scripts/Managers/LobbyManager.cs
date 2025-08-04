using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using System.Text;

/// <summary>
/// Stores basic data necesssary to know about all players in the lobby, without containing necessary information
/// for the game.
/// </summary>
[Serializable]
public class NetworkedPlayerLobbyInfo : INetworkSerializable
{
    public bool isHost;
    public ulong id;
    public string name;
    public BaseRpcTarget BaseRpcTarget => NetworkManager.Singleton.RpcTarget.Single(id, RpcTargetUse.Temp);
    
    // IMPORTANT: If you add a field in NetworkedPlayerLobbyInfo that needs to be transferred across the network,
    // it must be serialized here in the magic black box function.
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref isHost);
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref name);
    }

    // IMPORTANT: If you add a field that needs to remain in existence after the NetworkedPlayerLobbyInfo is converted into
    // a NetworkedPlayerGameplayInfo, it must be converted here.
    public static explicit operator NetworkedPlayerGameplayInfo(NetworkedPlayerLobbyInfo networkedPlayerLobbyInfo)
    {
        return new()
        {
            isHost = networkedPlayerLobbyInfo.isHost,
            id = networkedPlayerLobbyInfo.id,
            name = networkedPlayerLobbyInfo.name
        };
    }
}

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }
    [SerializeField] private ushort port;
    private float timerStartTime;
    [SerializeField] private float maxAttemptConnectAsClientWaitTime;
    public List<NetworkedPlayerLobbyInfo> NetworkedPlayerLobbyInfos { get; private set; }
    public NetworkedPlayerLobbyInfo LocalNetworkedPlayerLobbyInfo { get; private set; }
    [SerializeField] private FMODUnity.EventReference joinEvent;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        SoundMaster._instance.StartMusic("Lobby");
    }

    public static void DestroyInstance()
    {
        Destroy(Instance.gameObject);
        Instance = null;
    }

    private void ClientDisconnected(ulong id)
    {
        NetworkedPlayerLobbyInfo networkedPlayerLobbyInfo = GetNetworkedPlayerLobbyInfoOfId(id);
        NetworkedPlayerLobbyInfos.Remove(networkedPlayerLobbyInfo);
        AssignNewPlayerListRpc(NetworkedPlayerLobbyInfos.ToArray());
        if (MainCanvasController.Instance is LobbyCanvasController lobbyCanvasController)
        {
            lobbyCanvasController.RemovePlayerFromDisplay(networkedPlayerLobbyInfo);
            
            Debug.Log("Client disconnected. Disconnect applied to lobbyCanvasController");
        }
        else
        {
            Debug.Log("Could not find the LobbyCanvasController upon player disconnect");
        }
    }

    private void ServerStopped(bool _)
    {
        Debug.Log("Server stopped");
        LocalGameManager.Instance.LeaveLobby();
    }

    public void StartHost()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
            "127.0.0.1", // Local IP address
            port, // Port is a ushort
            "0.0.0.0" // Listen to all IP addressses
        );

        MainCanvasController.Instance.NetworkMessageText = "Starting host...";

        NetworkManager.Singleton.StartHost();

        if (!IsHost)
        {
            MainCanvasController.Instance.NetworkMessageText = "Hosting failed.";
            NetworkManager.Singleton.Shutdown();
            return;
        }

        MainCanvasController.Instance.NetworkMessageText = $"You are host! IP: {GetLocalIpAddress()}";
        InformServerOfSelf(true);

        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
    }

    private string GetLocalIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.ToString());
            return "Mac issue?";
        }
    }

    public void JoinIp(string ip)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, port);
        StartCoroutine(StartClientAndInform());
    }

    public void JoinLocalDebug()
    {
        StartCoroutine(StartClientAndInform());
    }

    private IEnumerator StartClientAndInform()
    {
        NetworkManager.Singleton.StartClient();
        timerStartTime = Time.time;
        MainCanvasController.Instance.NetworkMessageText = "Starting client...";
        yield return new WaitUntil(
            () => Time.time >= timerStartTime + maxAttemptConnectAsClientWaitTime || NetworkManager.Singleton.IsConnectedClient
            );
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            MainCanvasController.Instance.NetworkMessageText = "Joined!";
            InformServerOfSelf(false);
        }
        else
        {
            MainCanvasController.Instance.NetworkMessageText = "Client join failed, shutting down client join...";
            NetworkManager.Singleton.Shutdown();
            yield return new WaitUntil(() => !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsListening);
            MainCanvasController.Instance.NetworkMessageText = "Client join failed, network manager cleaned up.";
        }
        NetworkManager.Singleton.OnClientStopped += ServerStopped;
    }

    private void InformServerOfSelf(bool isHost)
    {
        LocalNetworkedPlayerLobbyInfo = new()
        {
            isHost = isHost,
            id = NetworkManager.Singleton.LocalClientId,
            name = PersistentDataManager.Instance.GetAccountName("Player")
        };
        if (MainCanvasController.Instance is LobbyCanvasController lobbyCanvasController)
        {
            lobbyCanvasController.SetLocalDisplayName(LocalNetworkedPlayerLobbyInfo.name);
        }
        else
        {
            Debug.LogWarning("LobbyManager could not find a valid LobbyCanvasController");
        }
        InformServerOfNewPlayerRpc(LocalNetworkedPlayerLobbyInfo);
    }

    [Rpc(SendTo.Server)]
    private void InformServerOfNewPlayerRpc(NetworkedPlayerLobbyInfo networkedPlayerLobbyInfo)
    {
        if (NetworkedPlayerLobbyInfos == null)
        {
            NetworkedPlayerLobbyInfos = new();
        }
        else
        {
            if (RemoveAllPlayersOfId(networkedPlayerLobbyInfo.id))
            {
                Debug.LogWarning($"Found a player with the same ID as the joining player! (name = {networkedPlayerLobbyInfo.name}, id = {networkedPlayerLobbyInfo.id})");
            }
        }

        string newName = GetNextAvailableName(networkedPlayerLobbyInfo.name);
        if (newName != networkedPlayerLobbyInfo.name)
        {
            networkedPlayerLobbyInfo.name = newName;
            InformUserOfNameChangeRpc(newName, networkedPlayerLobbyInfo.BaseRpcTarget);
        }
        NetworkedPlayerLobbyInfos.Add(networkedPlayerLobbyInfo);
        AssignNewPlayerListRpc(NetworkedPlayerLobbyInfos.ToArray());
    }

    private bool RemoveAllPlayersOfId(ulong id)
    {
        bool res = false;
        int i = 0;
        while (i < NetworkedPlayerLobbyInfos.Count)
        {
            if (id == NetworkedPlayerLobbyInfos[i].id)
            {
                NetworkedPlayerLobbyInfos.RemoveAt(i);
                res = true;
            }
            else
            {
                i++;
            }
        }
        return res;
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void InformUserOfNameChangeRpc(string newName, RpcParams rpcParams)
    {
        LocalNetworkedPlayerLobbyInfo.name = newName;
        if (MainCanvasController.Instance is LobbyCanvasController lobbyCanvasController)
        {
            lobbyCanvasController.SetLocalDisplayName(newName);
        }
        else
        {
            Debug.LogWarning("InformUserOfNameChangeRpc: LobbyManager could not find a valid LobbyCanvasController");
        }
    }

    private HashSet<string> GetAllTakenNames(List<NetworkedPlayerLobbyInfo> networkedPlayerLobbyInfos)
    {
        HashSet<string> res = new();
        foreach (NetworkedPlayerLobbyInfo networkedPlayerLobbyInfo in networkedPlayerLobbyInfos)
        {
            res.Add(networkedPlayerLobbyInfo.name);
        }
        return res;
    }

    private string GetNextAvailableName(string givenName)
    {
        int appendNum = 1;

        string newName = givenName;
        HashSet<string> takenNames = GetAllTakenNames(NetworkedPlayerLobbyInfos);
        while (takenNames.Contains(newName))
        {
            appendNum++;
            newName = $"{givenName} {appendNum}";
        }
        return newName;
    }

    [Rpc(SendTo.Everyone)]
    private void AssignNewPlayerListRpc(NetworkedPlayerLobbyInfo[] networkedPlayerLobbyInfos)
    {
        StringBuilder builder = new();
        builder.Append("New player list:\n");
        foreach (NetworkedPlayerLobbyInfo info in networkedPlayerLobbyInfos)
        {
            builder.Append($"{info.name}, ID {info.id}\n");
        }

        Debug.Log(builder.ToString());


        int countBeforeReassign; // Used to detect if this was a player join, at which point the player join effect plays
        if (NetworkedPlayerLobbyInfos == null)
        {
            countBeforeReassign = 0;
        }
        else
        {
            countBeforeReassign = NetworkedPlayerLobbyInfos.Count;
        }

        NetworkedPlayerLobbyInfos = networkedPlayerLobbyInfos.ToList();
        if (MainCanvasController.Instance is LobbyCanvasController lobbyCanvasController)
        {
            lobbyCanvasController.AssignNewPlayerList(NetworkedPlayerLobbyInfos);
            if (IsHost && networkedPlayerLobbyInfos.Length >= 2)
            {
                lobbyCanvasController.StartButtonVisible = true;
            }
        }

        if (NetworkedPlayerLobbyInfos.Count > countBeforeReassign) // If there are  now new players, play the effect
        {
            FMODUnity.RuntimeManager.PlayOneShot(joinEvent);
        }
    }

    public NetworkedPlayerLobbyInfo GetNetworkedPlayerLobbyInfoOfId(ulong id)
    {
        foreach (NetworkedPlayerLobbyInfo networkedPlayerLobbyInfo in NetworkedPlayerLobbyInfos)
        {
            if (networkedPlayerLobbyInfo.id == id)
            {
                return networkedPlayerLobbyInfo;
            }
        }
        throw new InvalidOperationException($"The requested networked player lobby info could not be found! (No player of ID {id} exists)");
    }
}

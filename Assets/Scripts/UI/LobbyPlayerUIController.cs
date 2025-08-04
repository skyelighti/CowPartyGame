using UnityEngine;
using TMPro;

public class LobbyPlayerUIController : MonoBehaviour
{
    // IMPORTANT: The comments in this code were copied from another project and will be implemented later.

    private NetworkedPlayerLobbyInfo netInfo;
    public NetworkedPlayerLobbyInfo NetworkedPlayerLobbyInfo
    {
        get
        {
            return netInfo;
        }
        set
        {
            netInfo = value;
            nameText.text = netInfo.name;
            /* To be implemented when lobby player buttons are implemented
            if (NetworkManager.Singleton.IsHost)
            {
                kickButton.SetActive(true);
            }
            if (networkedPlayerInfo.id == NetworkManager.Singleton.LocalClientId)
            {
                messageButton.SetActive(false);
                challengeButton.SetActive(false);
                kickButton.SetActive(false);
            }
            */
        }
    }
    [SerializeField] private TMP_Text nameText;

    /* To be implemented when lobby player buttons are implemented
    public void MessageButtonPressed()
    {
        if (SceneDataManager.Instance.mainCanvasController is LobbyCanvasController lobbyCanvasController)
        {
            if (lobbyCanvasController.chatController == null)
            {
                Debug.LogWarning("lobbyCanvasController's chatController is null.");
                return;
            }
            lobbyCanvasController.chatController.StartMessaging(networkedPlayerInfo.name);
        }
        else
        {
            Debug.LogWarning("The PlayerInLobbyUIController could not find a LobbyCanvasController.");
        }
    }

    public void ChallengeButtonPressed()
    {
        if (SceneDataManager.Instance.mainCanvasController is LobbyCanvasController lobbyCanvasController)
        {
            if (lobbyCanvasController.chatController == null)
            {
                Debug.LogWarning("lobbyCanvasController's chatController is null.");
                return;
            }
            lobbyCanvasController.chatController.Challenge(networkedPlayerInfo);
        }
        else
        {
            Debug.LogWarning("The PlayerInLobbyUIController could not find a LobbyCanvasController.");
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void KickButtonPressed()
    {
        LobbyManager.Instance.InformClientOfKickRpc(NetworkManager.Singleton.RpcTarget.Single(networkedPlayerInfo.id, RpcTargetUse.Temp));
        NetworkManager.Singleton.DisconnectClient(networkedPlayerInfo.id);
        LobbyManager.Instance.networkedPlayerInfos.Remove(networkedPlayerInfo);
        LobbyManager.Instance.ApplyNewPlayerListRpc(LobbyManager.Instance.networkedPlayerInfos.ToArray());
    }
    */

}

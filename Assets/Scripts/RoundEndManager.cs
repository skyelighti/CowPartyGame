using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundEndManager : NetworkBehaviour
{
    public static RoundEndManager Instance { get; private set; }

    private bool aliensWon;

    private NetworkedPlayerGameplayInfo localNetworkedPlayerGameplayInfo;

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
    }

    public static void DestroyInstance()
    {
        Destroy(Instance);
        Instance = null;
    }

    [Rpc(SendTo.Everyone)]
    public void ConfigureRoundEndRpc(bool aliensWon)
    {
        this.aliensWon = aliensWon;
        NetworkManager.Singleton.SceneManager.LoadScene("RoundEnd", LoadSceneMode.Single);
        localNetworkedPlayerGameplayInfo = NetworkedGameplayManager.Instance.LocalNetworkedPlayerGameplayInfo;
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (MainCanvasController.Instance is RoundEndCanvasController roundEndCanvasController)
        {
            roundEndCanvasController.SetVictor(aliensWon);
            // Get isAlien with localNetworkedPlayerGameplayInfo.isAlien
            if (IsHost)
            {
                roundEndCanvasController.ShowHostButtons();
            }
            if (localNetworkedPlayerGameplayInfo.isAlien == aliensWon)//play track based on win/lose
            {
                SoundMaster._instance.StartMusic("WinScene");
            }
            else
            {
                SoundMaster._instance.StartMusic("LoseScene");
            }
        }
        else
        {
            Debug.LogWarning("RoundEndManager could not find a valid RoundEndCanvasController");
        }
    }

    public void OnReturnPressed()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        DestroyInstance();
    }
}

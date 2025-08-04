using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Netcode;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocalGameManager : MonoBehaviour
{
    public static LocalGameManager Instance { get; private set; }

    /// <summary>
    /// A temporary variable for carrying the ip to join from a method to an event.
    /// This is a janky solution.
    /// </summary>
    private string ipToJoinTemp;

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

    public void StartLobbyAsHost()
    {
        // In order to tell the LobbyManager to start as host only after the lobby scene has been loaded,
        // SceneManager.sceneLoaded must be connected to a function that is called at scene load.
        // StartLobbyAsHost_OnSceneLoaded is called after the lobby is loaded.
        SceneManager.sceneLoaded += StartLobbyAsHost_OnSceneLoaded;
        SceneManager.LoadScene("Lobby");
    }

    private void StartLobbyAsHost_OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lobby")
        {
            LobbyManager.Instance.StartHost();
        }
        SceneManager.sceneLoaded -= StartLobbyAsHost_OnSceneLoaded;
    }

    public void JoinLobbyAsClient(string ip)
    {
        // ipToJoinTemp is a temporary variable that JoinLobbyAsClient_OnSceneLoaded will refer to, then null.
        ipToJoinTemp = ip;

        // In order to tell the LobbyManager to start as client only after the lobby scene has been loaded,
        // SceneManager.sceneLoaded must be connected to a function that is called at scene load.
        // JoinLobbyAsClient_OnSceneLoaded is called after the lobby is loaded.
        SceneManager.sceneLoaded += JoinLobbyAsClient_OnSceneLoaded;
        SceneManager.LoadScene("Lobby");
    }

    private void JoinLobbyAsClient_OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lobby")
        {
            LobbyManager.Instance.JoinIp(ipToJoinTemp);
            ipToJoinTemp = null;
        }
        SceneManager.sceneLoaded -= JoinLobbyAsClient_OnSceneLoaded;
    }

    public void JoinLocalDebug()
    {
        SceneManager.sceneLoaded += JoinLocalDebug_OnSceneLoaded;
        SceneManager.LoadScene("Lobby");
    }

    private void JoinLocalDebug_OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lobby")
        {
            LobbyManager.Instance.JoinLocalDebug();
        }
        SceneManager.sceneLoaded -= JoinLobbyAsClient_OnSceneLoaded;
    }

    public void ToGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            PersistentDataManager.Instance.GetGameSceneName(),
            LoadSceneMode.Single
        );
    }

    public void LeaveLobby()
    {
        StartCoroutine(nameof(LeaveLobbyCoroutine));
    }

    private IEnumerator LeaveLobbyCoroutine()
    {
        Debug.Log("Leave lobby coroutine");

        MainCanvasController.Instance.NetworkMessageText = "Shutting down network manager...";
        NetworkManager.Singleton.Shutdown();
        yield return new WaitUntil(() => !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsListening);
        MainCanvasController.Instance.NetworkMessageText = "Network manager cleaned up.";

        LobbyManager.DestroyInstance();

        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

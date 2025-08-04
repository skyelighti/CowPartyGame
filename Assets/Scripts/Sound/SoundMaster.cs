using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.SceneManagement;

public class SoundMaster : MonoBehaviour
{
    public static SoundMaster _instance { get; private set; }
    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [SerializeField] private EventReference lobby;
    [SerializeField] private EventReference win;
    [SerializeField] private EventReference lose;
    [SerializeField] private EventReference start;
    [SerializeField] private EventReference game;
    [SerializeField] private EventReference Countdown;

    private EventInstance lobbyStatic;
    private EventInstance winStatic;
    private EventInstance loseStatic;
    private EventInstance startStatic;
    private EventInstance gameStatic;
    private EventInstance CountdownStatic;

    public void StartMusic(string _SceneName)
    {
        //choose play mode
        string sceneName = _SceneName;
        SetEvent(ref lobbyStatic, lobby, sceneName == "Lobby" ? 1 : 0, "LobbyMod");
        SetEvent(ref winStatic, win, sceneName == "WinScene" ? 1 : 0, "WinMod");
        SetEvent(ref loseStatic, lose, sceneName == "LoseScene" ? 1 : 0, "LoseMod");
        SetEvent(ref startStatic, start, sceneName == "MainMenu" ? 1 : 0, "MenuMod");
        SetEvent(ref gameStatic, game, sceneName == "Game" ? 1 : 0, "GameMod");
    }
    void SetEvent(ref EventInstance inst, EventReference evt, int mod, string paramName)
    {
        if (mod > 0)
        {
            if (!inst.isValid())
                inst = RuntimeManager.CreateInstance(evt);
            inst.setParameterByName(paramName, mod);
            inst.start();
        }
        else
        {
            if (inst.isValid())
            {
                inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                inst.release();
                inst.clearHandle();
            }
        }
    }
    public void PlayCountdown()
    {
        if (!CountdownStatic.isValid())
        {
            CountdownStatic = RuntimeManager.CreateInstance(Countdown);
            CountdownStatic.start();
        }
    }
}

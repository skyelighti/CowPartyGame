using UnityEngine;
using FMODUnity;
using Unity.Netcode;
using System.Collections.Generic;
using System;

// 1‑A  List every sound you intend to broadcast
public enum SfxId
{
    abilities,
    countdown,
    cow,
    gamemusic,
    haybale,
    lobby,
    ufo_abduct,
    voiceAlien,
    voiceFarmer,
    walk,
    win,
    lose,
    start,
    UI_Start, UI_Click, UI_Join,
    hurt,
    take_it, walking_around, aliens_hiding, nice_shooting, comeon_now, we_dont// names are actually just the first two words
}
[System.Serializable]
public struct SfxEntry
{
    public SfxId id;
    public EventReference eventRef;
    public String modName;
}

public class NetSfx : NetworkBehaviour
{
    [Tooltip("Drag every networked SFX here once")]
    [SerializeField] public SfxEntry[] library;

    static NetSfx _instance;
    readonly Dictionary<SfxId, SfxEntry> _map = new();

    void Awake()
    {
        if (_instance && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        foreach (var e in library) _map[e.id] = e;
    }
    /// <summary>
    /// ids: abilities, countdown, cow, gamemusic, haybale, lobby, ufo, voiceAlien, voiceFarmer, walk, win, lose, start
    /// </summary>
    /// <param name="id">SfxId to play</param>
    /// <param name="worldPos">World position to play the sound at</param>
    /// <param name="mod">Optional parameter to modify the sound, e.g. volume or pitch. If one-shot then do not set</param>
    /// <returns>void</returns>
    public static void Play(SfxId id, Vector3 worldPos, int mod = -1)
    {
        if (_instance == null) { Debug.LogWarning("No NetSfx in scene"); return; }
        //_instance.PlayLocal(id, worldPos, mod);
        //if (NetworkClient.active)//mirror check
        _instance.CmdPlayServerRpc(id, worldPos, mod);
    }

    [ServerRpc]//end in ServerRpc cuz ngo
    void CmdPlayServerRpc(SfxId id, Vector3 pos, int mod)
    {
        RpcPlayClientRpc(id, pos, mod);
    }
    [ClientRpc]//same here
    void RpcPlayClientRpc(SfxId id, Vector3 pos, int mod)
    {
        // Host already played the predicted shot; skip to avoid double‑audio
        if (IsServer && IsClient) return;//decap I if we switch to mirror
        PlayLocal(id, pos, mod);
    }
    void PlayLocal(SfxId id, Vector3 pos, int mod)
    {
        if (!_map.TryGetValue(id, out var ev))
        {
            Debug.LogWarning($"NetSfx: {id} unknown"); return;
        }
        if (mod < 0)
        {
            RuntimeManager.PlayOneShot(ev.eventRef, pos);//FMOD 3‑D one‑shot
        }
        else
        {
            var inst = RuntimeManager.CreateInstance(ev.eventRef);
            inst.setParameterByName(ev.modName, mod);
            inst.start();
        }
    }
    public void PlayUFOBeamOneShot(Vector3 pos)
    {
        RuntimeManager.PlayOneShot("event:/UFO_Beam", pos);
    }
    //public override void OnStartServer()
    //{
    //    DontDestroyOnLoad(gameObject);
    //}

    //public override void OnStartClient()
    //{
    //    DontDestroyOnLoad(gameObject);
    //}
}
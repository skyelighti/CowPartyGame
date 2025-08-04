using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LobbyCanvasController : MainCanvasController
{
    [Header("Lobby Canvas Controller References")]
    [SerializeField] private RectTransform playerListRectTransform;
    [SerializeField] private GameObject startButtonGameObject;
    [SerializeField] private Transform lobbyPlayerRegion;
    [SerializeField] private GameObject lobbyPlayerPrefab;
    [SerializeField] private TMP_Text localNameText;
    private List<LobbyPlayerUIController> lobbyPlayerUIControllers;

    private bool startButtonVisible;

    public bool StartButtonVisible
    {
        get
        {
            return startButtonVisible;
        }
        set
        {
            if (value)
            {
                playerListRectTransform.offsetMin = new Vector2(playerListRectTransform.offsetMin.x, 55);
                startButtonGameObject.SetActive(true);
            }
            else
            {
                playerListRectTransform.offsetMin = new Vector2(playerListRectTransform.offsetMin.x, 0);
                startButtonGameObject.SetActive(false);
            }
            startButtonVisible = value;
        }
    }

    public void AssignNewPlayerList(List<NetworkedPlayerLobbyInfo> networkedPlayerLobbyInfos)
    {
        ClearPlayerList();
        foreach (NetworkedPlayerLobbyInfo networkedPlayerLobbyInfo in networkedPlayerLobbyInfos)
        {
            AddPlayer(networkedPlayerLobbyInfo);
        }
    }

    public void ClearPlayerList()
    {
        lobbyPlayerUIControllers = new();
        foreach (Transform childTransform in lobbyPlayerRegion)
        {
            Destroy(childTransform.gameObject);
        }
    }

    public void AddPlayer(NetworkedPlayerLobbyInfo networkedPlayerLobbyInfo)
    {
        GameObject newLobbyPlayer = Instantiate(lobbyPlayerPrefab, lobbyPlayerRegion);
        LobbyPlayerUIController lobbyPlayerUIController = newLobbyPlayer.GetComponent<LobbyPlayerUIController>();
        lobbyPlayerUIController.NetworkedPlayerLobbyInfo = networkedPlayerLobbyInfo;
        lobbyPlayerUIControllers.Add(lobbyPlayerUIController);
    }

    public void RemovePlayerFromDisplay(NetworkedPlayerLobbyInfo networkedPlayerLobbyInfo)
    {
        int c = 0;
        foreach (LobbyPlayerUIController lobbyPlayerUIController in lobbyPlayerUIControllers)
        {
            if (lobbyPlayerUIController.NetworkedPlayerLobbyInfo.id == networkedPlayerLobbyInfo.id)
            {
                Destroy(lobbyPlayerUIController.gameObject);
                c++;
            }
        }
        if (c == 0)
        {
            Debug.LogWarning($"Attempted to remove a player from display (id = {networkedPlayerLobbyInfo.id}, name = {networkedPlayerLobbyInfo.name}), but the player could not be found in any valid LobbyPlayerUIController!");
        }
        else if (c > 1)
        {
            Debug.LogWarning($"Attempted to remove a player from display (id = {networkedPlayerLobbyInfo.id}, name = {networkedPlayerLobbyInfo.name}), but {c} players with ID {networkedPlayerLobbyInfo.id} were found!");
        }
    }

    public void SetLocalDisplayName(string newName)
    {
        localNameText.text = newName;
    }

    public void OnStartButtonPressed()
    {
        StartCoroutine(OnStartTransitionWait());
    }

    private IEnumerator OnStartTransitionWait()
    {
        UIFXHandler.Instance.StartTransition(1, null, Color.black, true);
        yield return new WaitForSeconds(1);
        LocalGameManager.Instance.ToGame();
    }

    public void OnLeaveButtonPressed()
    {
        StartCoroutine(OnLeaveTransitionWait());
    }
    
    private IEnumerator OnLeaveTransitionWait()
    { 
        UIFXHandler.Instance.StartTransition(1, null, Color.black, true);
        yield return new WaitForSeconds(1);
        LocalGameManager.Instance.LeaveLobby();
    }

}

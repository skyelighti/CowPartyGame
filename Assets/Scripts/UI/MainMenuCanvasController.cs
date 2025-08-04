using System.Collections;
using TMPro;
using UnityEngine;

public class MainMenuCanvasController : MainCanvasController
{
    // Headers just make it look nicer in the inspector
    [Header("Main Menu Canvas Controller References")]
    [SerializeField] private GameObject TitlePanel;
    [SerializeField] private TMP_Text ipText;
    [SerializeField] private TMP_InputField gameSceneNameText;
    [SerializeField] private TMP_InputField accountNameText;

    private void Start()
    {
        AssignGameSceneNameText();
        AssignAccountDisplays();
        SoundMaster._instance.StartMusic("MainMenu");
        //CurrentPanel = TitlePanel;
    }

    private void AssignGameSceneNameText()
    {
        gameSceneNameText.text = PersistentDataManager.Instance.GetGameSceneName("");
    }

    private void AssignAccountDisplays()
    {
        accountNameText.text = PersistentDataManager.Instance.GetAccountName("");
    }

    public void OnHostButtonPressed()
    {
        StartCoroutine(HostTransitionWait());
    }

    private IEnumerator HostTransitionWait()
    { 
        UIFXHandler.Instance.StartTransition(1, null, Color.black, true);
        yield return new WaitForSeconds(1);
        LocalGameManager.Instance.StartLobbyAsHost();
    }

    public void OnJoinButtonPressed()
    {
        StartCoroutine(OnJoinTransitionWait());
    }

    private IEnumerator OnJoinTransitionWait()
    { 
        UIFXHandler.Instance.StartTransition(1, null, Color.black, true);
        yield return new WaitForSeconds(1);
        LocalGameManager.Instance.JoinLobbyAsClient(ipText.text[..^1]);
    }

    public void OnJoinLocalDebugButtonPressed()
    {
        StartCoroutine(OnJoinLocalDebugTransitionWait());
    }

    private IEnumerator OnJoinLocalDebugTransitionWait()
    { 
        UIFXHandler.Instance.StartTransition(1, null, Color.black, true);
        yield return new WaitForSeconds(1);
        LocalGameManager.Instance.JoinLocalDebug();
    }

    public void OnSaveAccountButtonPressed()
    {
        if (accountNameText.text == "")
        {
            PersistentDataManager.Instance.ClearAccountName();
        }
        else
        {
            PersistentDataManager.Instance.SaveAccountName(accountNameText.text);
        }
    }
    public void OnRevertAccountButtonPressed() => AssignAccountDisplays();
    public void OnBackAccountButtonPressed() => AssignAccountDisplays();


    public void OnSaveGameSceneOverridePressed()
    {
        Debug.Log($"Game scene saved as {gameSceneNameText.text}");
        PersistentDataManager.Instance.SaveGameSceneName(gameSceneNameText.text);
    }
    public void OnDeleteGameSceneOverridePressed()
    {
        PersistentDataManager.Instance.ClearGameSceneName();
        gameSceneNameText.text = PersistentDataManager.Instance.GetGameSceneName("");
    }
    public void OnQuitButtonPressed() => LocalGameManager.Instance.Quit();
}

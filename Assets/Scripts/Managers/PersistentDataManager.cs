using UnityEngine;

public class PersistentDataManager : MonoBehaviour
{
    public static PersistentDataManager Instance { get; private set; }

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

    public void SaveGameSceneName(string newName)
    {
        PlayerPrefs.SetString("GameSceneName", newName);
        PlayerPrefs.Save();
    }

    public void ClearGameSceneName()
    {
        PlayerPrefs.DeleteKey("GameSceneName");
        PlayerPrefs.Save();
    }

    public string GetGameSceneName()
    {
        return GetGameSceneName("Game");
    }

    public string GetGameSceneName(string defaultName)
    {
        if (PlayerPrefs.HasKey("GameSceneName"))
        {
            return PlayerPrefs.GetString("GameSceneName");
        }
        return defaultName;
    }

    public void SaveAccountName(string newName)
    {
        PlayerPrefs.SetString("AccountName", newName);
        PlayerPrefs.Save();
    }

    public void ClearAccountName()
    {
        PlayerPrefs.DeleteKey("AccountName");
        PlayerPrefs.Save();
    }

    public string GetAccountName(string defaultName)
    {
        if (PlayerPrefs.HasKey("AccountName"))
        {
            return PlayerPrefs.GetString("AccountName");
        }
        return defaultName;
    }
}

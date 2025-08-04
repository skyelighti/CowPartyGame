using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Texture2DArrayWrapper2
{
    public Sprite[] innerList;
}

public class RoundEndCanvasController : MainCanvasController
{
    [Header("Round End Settings")]
    //[SerializeField] private TMP_Text victoryText;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject hostButtons;

    [SerializeField] private Texture2DArrayWrapper2[] titleScreens;
    [SerializeField] private Image[] texInputs;

    public void SetVictor(bool aliensWon)
    {
        int chosenNum;
        if (!aliensWon)
        {
            chosenNum = 0;
        }
        else
        {
            chosenNum = 1;
        }

        Sprite[] chosenTheme = titleScreens[chosenNum].innerList;

        for (int i = 0; i < chosenTheme.Length; i++)
        {
            texInputs[i].sprite = chosenTheme[i];
        }
    }

    public void ShowHostButtons()
    {
        hostButtons.SetActive(true);
    }

    public void OnReturnPressed()
    {
        RoundEndManager.Instance.OnReturnPressed();
    }
}

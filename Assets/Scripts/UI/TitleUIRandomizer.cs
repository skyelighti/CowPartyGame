using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[System.Serializable]
public class Texture2DArrayWrapper
{
    public Texture2D[] innerList;
}
[System.Serializable]
public class StringArrayWrapper
{
    public String[] innerList;
}

public class TitleUIRandomizer : MonoBehaviour
{
    [SerializeField] private RawImage[] texInputs;
    [SerializeField] private Texture2DArrayWrapper[] titleScreens;

    [SerializeField] private StringArrayWrapper[] jokes;
    [SerializeField] private TextMeshProUGUI jokeText;

    void Start()
    {
        int chosenNum = UnityEngine.Random.Range(0, titleScreens.Length);
        Texture2D[] chosenTheme = titleScreens[chosenNum].innerList;

        for (int i = 0; i < chosenTheme.Length; i++)
        {
            texInputs[i].texture = chosenTheme[i];
        }
        jokeText.text = jokes[chosenNum].innerList[UnityEngine.Random.Range(0, jokes[chosenNum].innerList.Length)];
    }

    // Update is called once per frame
    void Update()
    {

    }
}

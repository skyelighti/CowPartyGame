using TMPro;
using UnityEngine;

public abstract class MainCanvasController : MonoBehaviour
{
    public static MainCanvasController Instance { get; private set; }


    // Headers just make it look nicer in the inspector
    [Header("Main Canvas Controller References")]
    [SerializeField] private TMP_Text networkMessage;

    private GameObject currentPanel = null;
    private GameObject currentCanvas;
    private bool sameCanvas;
    public GameObject CurrentPanel
    {
        get
        {
            return currentPanel;
        }
        set
        {
            if (currentPanel != null && sameCanvas) { currentPanel.SetActive(false); }
            currentPanel = value;
            //in setactive false to value's parents children
            foreach (Transform v in value.transform.parent.GetComponentInChildren<Transform>())
            {
                v.gameObject.SetActive(false);
            }
            currentPanel.SetActive(true);
        }
    }
    public GameObject CompareToCanvas
    {
        get
        {
            return currentCanvas;
        }
        set
        {
            if (currentCanvas == null) { currentCanvas = value; }
            sameCanvas = currentCanvas == value;
            currentCanvas = value;
        }
    }
    public string NetworkMessageText
    {
        get
        {
            return networkMessage.text;
        }
        set
        {
            networkMessage.text = value;
        }
    }

    private void Awake()
    {
        Instance = this;
    }
}

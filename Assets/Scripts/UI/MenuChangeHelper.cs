using UnityEngine;

public class MenuChangeHelper : MonoBehaviour
{
    [Header("Menu Change Settings")]
    [SerializeField] private GameObject panelToSwitchTo;
    [SerializeField] private GameObject currentCanvas;

    public void SwitchMenu()
    {
        if (panelToSwitchTo == null)
        {
            Debug.LogWarning("The panelToSwitchTo of the MenuChangeHelper is null!");
            return;
        }

        MainCanvasController.Instance.CompareToCanvas = currentCanvas;
        MainCanvasController.Instance.CurrentPanel = panelToSwitchTo;
    }
}
using System;
using System.Numerics;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ButtonUISettings
{
    public UnityEngine.Vector2 scale;
    public float lerpFactor;
}

public class ButtonUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform rectTransform;

    [Header("Settings")]
    [SerializeField] private ButtonUISettings startingSettings;
    [SerializeField] private ButtonUISettings hoverSettings;
    [SerializeField] private ButtonUISettings pressedSettings;
    [SerializeField] private float eventDelay;
    [SerializeField] private UnityEvent onPressedEvent;
    private ButtonUISettings targetSettings;


    private void Start()
    {
        targetSettings = startingSettings;
        MoveToSettings(startingSettings, 1f);
    }

    private void FixedUpdate()
    {
        MoveToSettings(targetSettings);
    }

    private void MoveToSettings(ButtonUISettings buttonUISettings)
    {
        MoveToSettings(buttonUISettings, buttonUISettings.lerpFactor);
    }

    private void MoveToSettings(ButtonUISettings buttonUISettings, float lerpFactor)
    {
        rectTransform.localScale = UnityEngine.Vector2.Lerp(rectTransform.localScale, buttonUISettings.scale, lerpFactor);
    }

    public void PointerEntered()
    {
        targetSettings = hoverSettings;
    }

    public void PointerExited()
    {
        targetSettings = startingSettings;
    }

    public void PointerDown()
    {
        targetSettings = pressedSettings;
        RuntimeManager.PlayOneShot("event:/UI Click");
        Invoke(nameof(ExecuteOnPointerDownEvent), eventDelay);
    }

    private void ExecuteOnPointerDownEvent() => onPressedEvent.Invoke();

    public void PointerUp()
    {
        targetSettings = startingSettings;
    }

    private void OnEnable()
    {
        targetSettings = startingSettings;
        MoveToSettings(targetSettings, 1f);
    }
}

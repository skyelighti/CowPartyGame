using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CamFXHandler : MonoBehaviour
{
    public static CamFXHandler Instance;

    public UniversalRendererData pcRenderer;

    //SPEED LINES
    public GameObject speedLines;

    // SCREEN TRANSITION
    public Material screenTransitions;
    private float transitionStartTime;
    [SerializeField] protected bool doSceneTransitionOnLoad = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            print("Destroyed CAMFX");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (doSceneTransitionOnLoad)
        {
            StartTransition(1, null, Color.black, false);
        }
    }

    public void EnableSpeedLines()
    {
        if (!speedLines.activeSelf)
            speedLines.SetActive(true);
    }

    public void DisableSpeedLines()
    {
        if (speedLines.activeSelf)
            speedLines.SetActive(false);
    }

    public void FreezeScreen(float duration)
    {
        if (Time.timeScale == 0) return;
        StartCoroutine(FreezeScreenCoroutine(duration));
    }
    IEnumerator FreezeScreenCoroutine(float duration)
    {
        var original = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = original;
    }

    public void StartTransition(float duration, Texture2D texture, Color color, bool isForward)
    {
        StartCoroutine(StartTransitionCoroutine(duration, texture, color, isForward));
    }
    public IEnumerator StartTransitionCoroutine(float duration, Texture2D texture, Color color, bool isForward)
    {
        if (isForward)
            screenTransitions.SetFloat("_Cutoff", 0f);
        else
            screenTransitions.SetFloat("_Cutoff", 1f);

        screenTransitions.SetColor("_FillColor", color);
        if (texture)
        {
            screenTransitions.SetTexture("_TransitionMap", texture);
        }

        transitionStartTime = Time.time;
        while (Time.time < transitionStartTime + duration)
        {
            float elapsedTime = Time.time - transitionStartTime;
            if (isForward)
                screenTransitions.SetFloat("_Cutoff", Mathf.Lerp(0f, 1f, elapsedTime / duration));
            else
                screenTransitions.SetFloat("_Cutoff", Mathf.Lerp(1f, 0f, elapsedTime / duration));
            yield return null;
        }

        if (isForward)
            screenTransitions.SetFloat("_Cutoff", 1f);
        else
            screenTransitions.SetFloat("_Cutoff", 0f);
    }
}

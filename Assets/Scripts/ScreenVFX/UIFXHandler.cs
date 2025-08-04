using UnityEngine;
using System.Collections;

public class UIFXHandler : MonoBehaviour
{
    public static UIFXHandler Instance;

    public Material screenTransitions;
    private float transitionStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartTransition(1, null, Color.black, false);
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

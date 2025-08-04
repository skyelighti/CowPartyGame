using System.Collections;
using UnityEngine;

public class UIPulser : MonoBehaviour
{
    private RectTransform uiTransform;
    private Vector3 originalSize;

    void Start()
    {
        uiTransform = transform.GetComponent<RectTransform>();
        originalSize = uiTransform.localScale;
    }
    public void PulseUI(float multiplier, float duration)
    {
        StartCoroutine(PulseUICoroutine(multiplier, duration));
    }
    private IEnumerator PulseUICoroutine(float multiplier, float duration)
    {

        //lerp from originalSiz*1.1 or sum to originalsize during the duration time

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            //Debug.Log($"Pulse elapsed time: {elapsedTime}");
            float t = elapsedTime / duration;
            uiTransform.localScale = Vector3.Lerp(originalSize*multiplier, originalSize, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        uiTransform.sizeDelta = originalSize;
    }
}

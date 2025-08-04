using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private CinemachineVirtualCameraBase virtualCamera;
    private CinemachineBasicMultiChannelPerlin perlinNoise;

    bool currentlyShaking;
    float currIntensity;
    float totalDuration;
    float startTime;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCameraBase>();
        perlinNoise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        //ShakeCameraLerp(10, 2);
        //ShakeCameraStatic(10, 20);
    }

    public void ShakeCameraLerp(float intensity, float duration)
    {
        if (currentlyShaking) StopCoroutine(ShakeCamLerp(currIntensity, totalDuration));
        StartCoroutine(ShakeCamLerp(intensity, duration));
    }

    public void ShakeCameraStatic(float intensity, float duration)
    {
        if (currentlyShaking) StopCoroutine(ShakeCamStatic(currIntensity, totalDuration));
        StartCoroutine(ShakeCamStatic(intensity, duration));
    }

    private IEnumerator ShakeCamStatic(float intensity, float duration)
    {
        currentlyShaking = true;
        currIntensity = intensity;
        totalDuration = duration;
        perlinNoise.AmplitudeGain = intensity;
        yield return new WaitForSeconds(duration);
        perlinNoise.AmplitudeGain = 0f;
        currentlyShaking = false;
    }

    private IEnumerator ShakeCamLerp(float intensity, float duration)
    {
        currentlyShaking = true;
        currIntensity = intensity;
        totalDuration = duration;
        startTime = Time.time;
        perlinNoise.AmplitudeGain = intensity;

        while (Time.time < startTime + duration)
        {
            float elapsedTime = Time.time - startTime;
            perlinNoise.AmplitudeGain = Mathf.Lerp(intensity, 0f, elapsedTime / duration);
            yield return null;
        }

        currentlyShaking = false;
        perlinNoise.AmplitudeGain = 0f;
    }
}

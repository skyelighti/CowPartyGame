using UnityEngine;

public class refreshCamera : MonoBehaviour
{
    public GameObject mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera.SetActive(false);
        mainCamera.SetActive(true);
    }
}

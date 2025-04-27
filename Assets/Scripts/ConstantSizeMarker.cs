using UnityEngine;

public class ConstantSizeMarker : MonoBehaviour
{
    public float screenSize = 20f; // Pixels
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        transform.localScale = Vector3.one * screenSize;
    }

    void Update()
    {
        if (!mainCamera) return;

        // Face camera and maintain size
        transform.forward = mainCamera.transform.forward;
        transform.localScale = Vector3.one *
            (screenSize * Vector3.Distance(transform.position, mainCamera.transform.position)
             / mainCamera.fieldOfView);
    }
}

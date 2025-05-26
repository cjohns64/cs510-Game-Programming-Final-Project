using UnityEngine;

public class BackToShipButtonVisibility : MonoBehaviour
{
    private ThirdPersonCameraController _controller;

    void Shutdown(string message = "error")
    {
        Debug.Log("[BackToShipButton] " + message);
        enabled = false;
        return;
    }

    void Start()
    {
        var camera = Camera.main;
        if (camera == null) Shutdown("No main camera");

        _controller = camera.GetComponent<ThirdPersonCameraController>();
        if (_controller == null) Shutdown("No ThridPersonCameraController component");

    }

    void Update()
    {
        bool shouldShow = !_controller.AtMainTarget();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf != shouldShow)
                child.gameObject.SetActive(shouldShow);
        }
    }
}

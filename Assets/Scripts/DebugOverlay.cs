using UnityEngine;
using TMPro;

public class DebugOverlay : MonoBehaviour
{
    public GameObject debugPanel;
    public TMP_Text fpsText;

    private bool showDebug = false;
    private float deltaTime = 0.0f;

    void Start()
    {
        debugPanel.SetActive(false);
    }

    void Update()
    {
        // Toggle with F3
        if (Input.GetKeyDown(KeyCode.F3))
        {
            showDebug = !showDebug;
            debugPanel.SetActive(showDebug);
        }

        if (showDebug)
        {
            // FPS calculation
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsText.text = $"FPS: {fps:0.0}";
        }
    }
}

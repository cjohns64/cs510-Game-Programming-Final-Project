using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Timeometer : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The component that drives ship motion and exposes current speed.")]
    [SerializeField] private TimeController controller;
    [SerializeField] private bool autoFindMoverIfNull = true;

    [Tooltip("UI Text field to display speed.")]
    [SerializeField] private TextMeshProUGUI timeText;

    void Awake()
    {
        // Try auto‑assign if missing
        if (timeText == null)
            timeText = GetComponent<TextMeshProUGUI>();

        if (autoFindMoverIfNull && controller == null)
            controller = GameObject.FindWithTag("TimeController")?.GetComponent<TimeController>();

        if (controller == null || timeText == null)
            {
                Debug.LogError($"[{nameof(Timeometer)}] Missing references. Disabling.");
                enabled = false;
                return;
            }
    }

    void Update()
    {
        timeText.SetText("Timewarp: {0:0}x", controller.GetTimeScale());
    }
}

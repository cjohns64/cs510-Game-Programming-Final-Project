using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Speedometer : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The component that drives ship motion and exposes current speed.")]
    [SerializeField] private OrbitMoverAnalytic mover;
    [SerializeField] private bool autoFindMoverIfNull = true;

    [Tooltip("UI Text field to display speed.")]
    [SerializeField] private TextMeshProUGUI speedText;

    void Awake()
    {
        // Try auto‑assign if missing
        if (speedText == null)
            speedText = GetComponent<TextMeshProUGUI>();

        if (autoFindMoverIfNull && mover == null)
            mover = GameObject.FindWithTag("Ship")?.GetComponent<OrbitMoverAnalytic>();

        if (mover == null || speedText == null)
            {
                Debug.LogError($"[{nameof(Speedometer)}] Missing references. Disabling.");
                enabled = false;
                return;
            }
    }

    void Update()
    {
        // Direct, zero‑allocation formatting
        speedText.SetText("{0:2} u/sec", mover.state.speed);
    }
}

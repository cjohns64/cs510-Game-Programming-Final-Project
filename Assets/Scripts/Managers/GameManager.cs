using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private OrbitMoverAnalytic shipMover;
    [SerializeField] private TradeManager tradeManager;

    private float previousTimeScale;

    [SerializeField] private TimeController timeController;
    [SerializeField] private float docking_time_interval = 10.0f;
    private float docking_timer = 0.0f;

    void OnEnable()
    {
        shipMover.OnSOITransition += HandleSOITransition;
        tradeManager.OnMenuClosed += HandleMenuClosed;
    }

    void OnDisable()
    {
        shipMover.OnSOITransition -= HandleSOITransition;
        tradeManager.OnMenuClosed -= HandleMenuClosed;
    }

    private void Update()
    {
        docking_timer += Time.deltaTime;
    }

    private void HandleSOITransition(CelestialBody newBody)
    {
        if (docking_timer > docking_time_interval)
        {
            // 1) Pause time
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            // 2) Show trade UI
            tradeManager.OpenMenu(newBody);
        }
        
    }

    private void HandleMenuClosed(CelestialBody landedBody)
    {
        PlaceShipInOrbit(landedBody);
        timeController.SetTimeScale();
        docking_timer = 0.0f;
    }

    private void PlaceShipInOrbit(CelestialBody body)
    {
        Vector3 direction = (body.transform.position - shipMover.CentralBody.position).normalized;
        Vector3 positionOutsideSOI = body.transform.position + direction * body.SoiRadius * 1.1f;
        shipMover.SetPosition(positionOutsideSOI);
    }
}

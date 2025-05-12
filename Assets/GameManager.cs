using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private OrbitMoverAnalytic shipMover;
    [SerializeField] private TradeManager tradeManager;

    private float previousTimeScale;

    [SerializeField] private TimeController timeController;

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

    private void HandleSOITransition(CelestialBody newBody)
    {
        // 1) Pause time
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // 2) Show trade UI
        tradeManager.OpenMenu(newBody);
    }

    private void HandleMenuClosed(CelestialBody landedBody)
    {
        PlaceShipInOrbit(landedBody);
        timeController.SetTimeScale();
    }

    private void PlaceShipInOrbit(CelestialBody body)
    {
        Vector3 direction = (body.transform.position - shipMover.CentralBody.position).normalized;
        Vector3 positionOutsideSOI = body.transform.position + direction * body.SoiRadius * 1.1f;
        shipMover.SetPosition(positionOutsideSOI);
    }
}

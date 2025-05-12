using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private OrbitMoverAnalytic shipMover;
    [SerializeField] private TradeManager tradeManager;

    private float previousTimeScale;

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
        Debug.Log("CLOSE");
        PlaceShipInOrbit(landedBody);

        Time.timeScale = previousTimeScale;
    }

    private void PlaceShipInOrbit(CelestialBody body)
    {
        float radius = body.SoiRadius * 1.05f;
        Vector3 offset = new Vector3(radius, 0, 0); 

        Vector3 spawnPos = body.transform.position + 
                           body.transform.rotation * offset;

        shipMover.transform.position = spawnPos;

        shipMover.CentralBody = body.transform;
        shipMover.initializationType = OrbitMoverAnalytic.InitializationType.CircularOrbit;
        shipMover.Start(); 
    }
}

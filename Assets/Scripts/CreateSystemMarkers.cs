using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateSystemMarkers : MonoBehaviour
{
    [SerializeField] private CelestialBody mainCelestialBody;
    [SerializeField] private GameObject systemMarkerPrefab;

    [SerializeField] private float markerDistanceFactor = 1.1f;

    private GalaxyDatabase _galaxyDatabase;

    void Start()
    {
        _galaxyDatabase = Resources.Load<GalaxyDatabase>("GalaxyDatabase");
        if (_galaxyDatabase == null)
        {
            Debug.LogError("GalaxyDatabase not found in Resources.");
            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        SolarSystemData currentSystem = _galaxyDatabase.GetSystemByName(currentSceneName);
        if (currentSystem == null)
        {
            Debug.LogError($"Current system '{currentSceneName}' not found in GalaxyDatabase.");
            return;
        }

        foreach (var targetSystem in _galaxyDatabase.allSystems)
        {
            if (targetSystem == currentSystem)
                continue;

            Vector3 galacticDirection = (targetSystem.galacticPosition - currentSystem.galacticPosition).normalized;

            Vector3 markerPosition = mainCelestialBody.transform.position + galacticDirection * mainCelestialBody.SoiRadius * markerDistanceFactor;

            GameObject marker = Instantiate(systemMarkerPrefab, markerPosition, Quaternion.identity, this.transform);
            marker.name = $"{targetSystem.systemName} Marker";
        }
    }

    void Update()
    {
        
    }
}

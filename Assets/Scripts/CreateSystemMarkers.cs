using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CreateSystemMarkers : MonoBehaviour
{
    [SerializeField] private CelestialBody mainCelestialBody;
    [SerializeField] private GameObject systemMarkerPrefab;
    [SerializeField] private GameObject systemTravelTriggerPrefab;
    [SerializeField] private string currentSceneName;

    [SerializeField] private float markerDistanceFactor = 1.1f;
    [SerializeField] private float markerSize = 5f;
    [SerializeField] private float triggerSize = 20f;

    private GalaxyDatabase _galaxyDatabase;

    void Start()
    {
        _galaxyDatabase = Resources.Load<GalaxyDatabase>("GalaxyDatabase");
        if (_galaxyDatabase == null)
        {
            Debug.LogError("GalaxyDatabase not found in Resources.");
            return;
        }

        //string currentSceneName = SceneManager.GetActiveScene().name;
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

            Vector3 direction = (targetSystem.galacticPosition - currentSystem.galacticPosition).normalized;
            float distance = Vector3.Distance(currentSystem.galacticPosition, targetSystem.galacticPosition);
            distance /= 10;

            // Make the system marker
            Vector3 markerPosition = mainCelestialBody.transform.position + direction * mainCelestialBody.SoiRadius * markerDistanceFactor;

            GameObject marker = Instantiate(systemMarkerPrefab, markerPosition, Quaternion.identity, this.transform);
            marker.name = $"{targetSystem.systemName} Marker";

            marker.transform.localScale = Vector3.one * markerSize;

            TextMeshPro textUI = marker.GetComponentInChildren<TextMeshPro>();
            if (textUI != null)
            {
                textUI.text = $"{targetSystem.systemName}\n{distance:F2} ly";
            }
            else
            {
                Debug.LogWarning($"Marker prefab '{systemMarkerPrefab.name}' is missing a TextMeshPro component.");
            }

            // Make the travel trigger
            Vector3 triggerPosition = mainCelestialBody.transform.position + direction * mainCelestialBody.SoiRadius * 1f;
            GameObject trigger = Instantiate(systemTravelTriggerPrefab, triggerPosition, Quaternion.identity, this.transform);
            trigger.name = $"{targetSystem.systemName} Travel Trigger";
            trigger.transform.localScale = Vector3.one * triggerSize;
            trigger.GetComponent<InterstellarTravel>().target_system_name = targetSystem.systemName;
        }
    }

}

using System;
using UnityEngine;

public class InterstellarTravel : MonoBehaviour
{
    private SaveManagerInterface saveManager;
    private GalaxyDatabase _galaxyDatabase;
    public string target_system_name;
    public bool is_preloader = false;

    [Header("Default settings")]
    [SerializeField] private string global_scripts = "GlobalScripts";
    private void Start()
    {
        saveManager = GameObject.Find(global_scripts).GetComponent<SaveManagerInterface>();
        _galaxyDatabase = Resources.Load<GalaxyDatabase>("GalaxyDatabase");
        if (_galaxyDatabase == null)
        {
            Debug.LogError("GalaxyDatabase not found in Resources.");
            return;
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ship")) return;

        if (!is_preloader)
        {
            Debug.Log($"Entering interstellar tunnel to {target_system_name}");
            // load other scene
            saveManager.PreloadActivate();
            
        }
        if (is_preloader)
        {
            Debug.Log($"Preparing interstellar tunnel to {target_system_name}");
            // start loading scene
            saveManager.ScenePreloadByIndex(_galaxyDatabase.GetSystemByName(target_system_name).system_index);
        }
    }
}

using System;
using UnityEngine;

public class InterstellarTravel : MonoBehaviour
{
    private SaveManagerInterface saveManager;
    private GalaxyDatabase _galaxyDatabase;
    public string target_system_name;

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

        //string[] words = gameObject.name.Split(' ');
        //string systemName = words.Length > 1
        //    ? string.Join(" ", words, 0, words.Length - 1)
        //    : gameObject.name;
        Debug.Log($"Entering interstellar tunnel to {target_system_name}");
        // load other scene
        saveManager.SceneTransitionByIndex(_galaxyDatabase.GetSystemByName(target_system_name).system_index);
    }
}

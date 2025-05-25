using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/**
 * Interfaces with the associated LocalSceneSaveManager for this scene
 */
public class SaveManagerInterface : MonoBehaviour
{
    [Header("Required Resources")]
    [SerializeField] private LocalSceneSaveManager save_manager;
    // player ship model, the active state of each mesh component is needed
    [SerializeField] private GameObject player_ship;
    // player inventory
    [SerializeField] private Inventory player_inventory;
    // player ship mesh status, will check the enabled state on all upgrades
    //public Dictionary<string, bool> mesh_status = new();
    [SerializeField] private UpgradeManager upgrade_manager;
    // upgrade menu dropdown settings
    [SerializeField] private Inventory[] planet_inventories;

    private List<AsyncOperation> scenesToLoad = new();

    //https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html
    private void Start()
    {
        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void SceneTransitionByIndex(int index)
    {
        //SaveSceneData();
        //scenesToLoad.Clear();
        scenesToLoad.Add(SceneManager.LoadSceneAsync("Zone " + index.ToString()));
    }

    // trigger data loading once the scene has finished loading
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadSceneData();
    }

    public void SaveSceneData()
    {
        save_manager.SaveData(upgrade_manager, player_ship, player_inventory, planet_inventories);
        Debug.Log("Scene Data Saved");
    }

    public void LoadSceneData()
    {
        save_manager.LoadData(upgrade_manager, player_ship, player_inventory, planet_inventories);
        Debug.Log("Scene Data Loaded");
    }
}

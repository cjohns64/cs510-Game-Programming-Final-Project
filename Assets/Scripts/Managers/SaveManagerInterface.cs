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
    [SerializeField] private StatManager player_stats;
    private bool preload_wait = false;

    //https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html
    private void Start()
    {
        // load data from previous scene
        LoadSceneData();
    }

    public void SceneTransitionByIndex(int index)
    {
        StartCoroutine(LoadByIndex(index));
    }

    public void ScenePreloadByIndex(int index)
    {
        preload_wait = true;
        StartCoroutine(LoadByIndex(index));
    }

    public void PreloadActivate()
    {
        preload_wait = false;
    }

    private IEnumerator LoadByIndex(int index)
    {
        yield return null;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Zone " + index.ToString());
        asyncOperation.allowSceneActivation = false;
        // wait until save is done to load next scene
        SaveSceneData();
        while (!asyncOperation.isDone)
        {
            if (asyncOperation.progress >= 0.9f && !preload_wait)
            {
                asyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    public void SaveSceneData()
    {
        save_manager.SaveData(upgrade_manager, player_ship, player_inventory, planet_inventories, player_stats.SaveHealth());
        Debug.Log("Scene Data Saved");
    }

    public void LoadSceneData()
    {
        save_manager.LoadData(upgrade_manager, player_ship, player_inventory, planet_inventories);
        player_stats.LoadHealth(save_manager.LoadHealth());
        Debug.Log("Scene Data Loaded");
    }
}

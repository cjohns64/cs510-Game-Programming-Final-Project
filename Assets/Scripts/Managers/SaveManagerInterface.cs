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
    private float timer = 0f;
    private float maxTime = 3f;
    private bool hasfired = false;
    private bool counting = false;

    //https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html
    private void Start()
    {
        //SceneManager.sceneLoaded += OnSceneLoaded;
        LoadSceneData();
    }

    public void SceneTransitionByIndex(int index)
    {
        StartCoroutine(LoadByIndex(index));
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
            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    // trigger data loading once the scene has finished loading
    //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    hasfired = true;
    //    //LoadSceneData();
    //}

    private void Update()
    {
        // delay LoadSceneData by maxTime
        if (hasfired)
        {
            counting = true;
            Debug.Log("Started Counting");
        }
        if (counting)
        {
            timer += Time.deltaTime;
        }
        if (timer > maxTime)
        {
            Debug.Log("Done Counting");
            hasfired = false;
            counting = false;
            LoadSceneData();
        }
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

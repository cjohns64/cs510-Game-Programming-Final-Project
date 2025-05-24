using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Manages Saving and Loading data to a specific scene, as well as transferring data to a global scene manager
 */
[CreateAssetMenu(fileName = "LocalSceneSaveManager", menuName = "Scriptable Objects/LocalSceneSaveManager")]
public class LocalSceneSaveManager : ScriptableObject
{
    [Header("Required Resources")]
    public GlobalSaveManager global_save_manager;
    public GameObject player_ship;

    [Header("Resident Inventory Data")]
    // all inventories whose data should be saved but not transferred
    public Inventory[] resident_inventories;

    [Header("Orbit Data")]
    public GameObject[] saved_bodies;

    [Header("Persistent Data")]
    // data that needs to transfer between scenes
    // player inventory
    public Inventory player_inventory;
    // player ship mesh status, will check the enabled state on all upgrades
    //public Dictionary<string, bool> mesh_status = new();
    public UpgradeManager upgrade_manager;
    // upgrade menu dropdown settings

    // load data from global save manager
    public void LoadData()
    {
        // TODO Orbit data
        // TODO upgrade menu dropdown values
        // player ship mesh active state
        LoadMeshState(player_ship);
        // player inventory items
        player_inventory.SetInventory(global_save_manager.LoadPlayerInventory());
    }

    // save data to global save manager
    public void SaveData()
    {
        // TODO Orbit data
        // TODO upgrade menu dropdown values
        // player ship mesh active state
        SaveMeshState(player_ship);
        // player inventory items
        foreach (ItemType type in ItemType.GetValues(typeof(ItemType)))
        {
            // save all player items to global save manager
            global_save_manager.SavePlayerItem(type, player_inventory.GetItemAmount(type));
        }
    }

    private void SaveMeshState(GameObject mesh, string path="")
    {
        for (int i=0; i<mesh.transform.childCount; i++)
        {
            GameObject current_child = mesh.transform.GetChild(i).gameObject;
            // add object state to global save data
            global_save_manager.SaveMeshActiveState(path + current_child.name, current_child.activeSelf);
            // check for more children
            if (current_child.transform.childCount == 0)
            {
                // base case
                continue;
            }
            else
            {
                // recursive call
                SaveMeshState(current_child, path + current_child.name);
            }
        }
    }

    private void LoadMeshState(GameObject mesh, string path = "")
    {
        for (int i = 0; i < mesh.transform.childCount; i++)
        {
            GameObject current_child = mesh.transform.GetChild(i).gameObject;
            // get the object state from global save data
            bool state = global_save_manager.LoadMeshActiveState(path + current_child.name);
            // set this object's state
            current_child.SetActive(state);
            // check for more children
            if (current_child.transform.childCount == 0)
            {
                // base case
                continue;
            }
            else
            {
                // recursive call
                SaveMeshState(current_child, path + current_child.name);
            }
        }
    }
}

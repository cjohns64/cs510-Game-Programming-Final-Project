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
    [SerializeField] private GlobalSaveManager global_save_manager;

    [Header("Script Settings")]
    public bool contains_local_saved_data = false;

    private List<Dictionary<ItemType, int>> saved_inventories = new();
    // load data from global save manager
    public void LoadData(UpgradeManager upgrade_manager,
                        GameObject player_ship,
                        Inventory player_inventory,
                        Inventory[] planet_inventories)
    {
        if (global_save_manager.contains_saved_data)
        {
            // TODO Orbit data
            // upgrade menu dropdown values
            upgrade_manager.LoadDropdownSettings(global_save_manager.LoadDropdownValues());
            // player ship mesh active state
            LoadMeshState(player_ship);
            // player inventory items
            player_inventory.SetInventory(global_save_manager.LoadPlayerInventory());
        }
        if (contains_local_saved_data)
        {
            // non-scene persistent data that should still be save for the next time this scene is loaded
            // like planet inventories and positions
            // load all planet inventories
            for (int i=0; i<planet_inventories.Length; i++)
            {
                planet_inventories[i].SetInventory(saved_inventories[i]);
            }
        }
    }

    // save data to global save manager
    public void SaveData(UpgradeManager upgrade_manager,
                        GameObject player_ship,
                        Inventory player_inventory,
                        Inventory[] planet_inventories,
                        float current_hull)
    {
        // TODO Orbit data
        // ship health
        global_save_manager.SaveHealth(current_hull);
        // upgrade menu dropdown values
        global_save_manager.SaveDropdownValues(upgrade_manager.SaveDropdownSettings());
        // player ship mesh active state
        SaveMeshState(player_ship);
        // player inventory items
        foreach (ItemType type in ItemType.GetValues(typeof(ItemType)))
        {
            // save all player items to global save manager
            global_save_manager.SavePlayerItem(type, player_inventory.GetItemAmount(type));
        }
        // saved global data
        global_save_manager.contains_saved_data = true;

        // save planet inventories
        for (int i = 0; i < planet_inventories.Length; i++)
        {
            foreach (ItemType type in ItemType.GetValues(typeof(ItemType)))
            {
                // save all planet items to local save manager
                SaveItem(i, type, planet_inventories[i].GetItemAmount(type));
            }
        }
        // saved local data
        contains_local_saved_data = true;
    }

    public void SaveItem(int inventory_index, ItemType item, int quantity)
    {
        //Debug.Log(item.ToString() + " " + inventory_index.ToString() + " " + quantity.ToString());
        // check if list contains an entry for the index
        if (saved_inventories.Count <=  inventory_index)
        {
            // no entry, add new dictionary, assumes indexes are sorted
            saved_inventories.Add(new Dictionary<ItemType, int> { { item, quantity } });
        }
        // check if dictionary at list index contains the item
        else if (saved_inventories[inventory_index].ContainsKey(item))
        {
            // item is in dictionary, update its value
            saved_inventories[inventory_index][item] = quantity;
        }
        else
        {
            // add new item
            saved_inventories[inventory_index].Add(item, quantity);
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

    public float LoadHealth()
    {
        if (global_save_manager.contains_saved_data)
        {
            return global_save_manager.GetHealth();
        }
        return 100;
    }
}

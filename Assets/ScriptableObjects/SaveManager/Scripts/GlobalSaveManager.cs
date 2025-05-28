using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Stores data that needs to pass between scenes
 */
[CreateAssetMenu(fileName = "GlobalSaveManager", menuName = "Scriptable Objects/GlobalSaveManager")]
public class GlobalSaveManager : ScriptableObject
{
    [Header("Script Settings")]
    public bool contains_saved_data = false;

    private List<int> dropdown_values = new();
    private Dictionary<string, bool> ship_mesh_active_state = new();
    private Dictionary<ItemType, int> player_inventory_items = new();

    public void SaveDropdownValues(List<int> settings)
    {
        dropdown_values = settings;
    }

    public List<int> LoadDropdownValues()
    {
        return dropdown_values;
    }

    public void SaveMeshActiveState(string key, bool value)
    {
        if (ship_mesh_active_state.ContainsKey(key))
        {
            ship_mesh_active_state[key] = value;
        }
        else
        {
            ship_mesh_active_state.Add(key, value);
        }
    }

    public bool LoadMeshActiveState(string key)
    {
        if (ship_mesh_active_state.ContainsKey(key))
        {
            return ship_mesh_active_state[key];
        }
        return false;
    }

    public void SavePlayerItem(ItemType item, int quantity)
    {
        if (player_inventory_items.ContainsKey(item))
        {
            player_inventory_items[item] = quantity;
        }
        else
        {
            player_inventory_items.Add(item, quantity);
        }
    }

    public Dictionary<ItemType, int> LoadPlayerInventory()
    {
        return player_inventory_items;
    }
}

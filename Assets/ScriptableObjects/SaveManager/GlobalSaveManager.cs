using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Stores data that needs to pass between scenes
 */
[CreateAssetMenu(fileName = "GlobalSaveManager", menuName = "Scriptable Objects/GlobalSaveManager")]
public class GlobalSaveManager : ScriptableObject
{
    [SerializeField] private Dictionary<string, int> dropdown_values;
    [SerializeField] private Dictionary<string, bool> ship_mesh_active_state;
    [SerializeField] private Dictionary<ItemType, int> player_inventory_items;

    public void SaveDropdownValue(string key, int value)
    {
        if (dropdown_values.ContainsKey(key))
        {
            dropdown_values[key] = value;
        }
        else
        {
            dropdown_values.Add(key, value);
        }
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

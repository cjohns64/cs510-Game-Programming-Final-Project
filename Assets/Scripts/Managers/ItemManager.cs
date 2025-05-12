using System.Collections.Generic;
using UnityEngine;

/**
 * The Item manager is the source of all item prefabs.
 * Other scripts can lookup or reference this script to get a list of all items in the game
 * and their associated prefab.
 * 
 * It works by defining an array of ItemObjects for filling out in the inspector.
 * Then converting this array into a dictionary for lookup.
 */
public class ItemManager : MonoBehaviour
{
    // base prefab used for all items, its sprite and text areas are changed after it is instanced
    public GameObject item_ui_prefab;
    [SerializeField] private ItemObject[] all_item_prefabs; // references to all items in the game
    private Dictionary<ItemType, ItemObject> internal_item_database = new Dictionary<ItemType, ItemObject>();
    // dictionary must be initialized for system to work
    private bool is_initialized = false;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        // fill internal database with inspector defined item list
        foreach (ItemObject item in all_item_prefabs)
        {
            internal_item_database[item.type] = item;
            //Debug.Log("Add " + item.type.ToString());
        }
        is_initialized = true;
        //Debug.Log("Initialized Item Manager to size " + internal_item_database.Count);
    }

    public ItemObject GetItem(ItemType type)
    {
        if (!is_initialized)
        {
            // initialize the database if it has not been initialized
            // Awake should call this before, but check again just to be sure
            InitializeDictionary();
        }
        // get type from database, or null if it is not in the database
        return internal_item_database.TryGetValue(type, out ItemObject item) ? item : null;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // actual inventory
    private Dictionary<ItemType, int> items = new Dictionary<ItemType, int>();

    // for defining the default inventory in the inspector
    [Header("Starting Inventory - Item & quantity")]
    [SerializeField] private InventorySlot[] defined_default_inv;

    [Header("Production/Consumption Lists - Item & produced/consumed amounts")]
    [SerializeField] private InventorySlot[] produces;
    [SerializeField] private InventorySlot[] consumes;

    [Header("Trading Metadata")]
    public int inventory_max_capacity = 1000;
    public int inventory_current_capacity = 0;

    public event Action<ItemType> OnInventoryChanged;
    private ItemManager item_manager;

    private void Start()
    {
        item_manager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        InitInventory();
    }

    /**
     * Loads inventory with given values
     */
    public void SetInventory(Dictionary<ItemType, int> inventory_state)
    {
        foreach (ItemType item in inventory_state.Keys)
        {
            if (this.items.ContainsKey(item))
            {
                this.items[item] = inventory_state[item];
            }
            else
            {
                this.AddItem(item, inventory_state[item]);
            }
        }
    }

    /**
     * Sets up the inventory with the values defined in the inspector.
     * Unless the items dictionary already has values.
     */
    private void InitInventory()
    {
        if (items.Count == 0) // don't add items unless actual items dict is uninitialized
        {
            // setup inventory
            // use defined_default_inv
            for (int i = 0; i < defined_default_inv.Length; i++)
            {
                // add item to actual inventory
                AddItem(defined_default_inv[i].item, defined_default_inv[i].amount);
                inventory_current_capacity += defined_default_inv[i].amount * 
                    item_manager.GetItem(defined_default_inv[i].item).item_size;
            }
        }
    }

    // checks if inventory has space for new items
    public bool HasCapacity(int add_amount)
    {
        return inventory_current_capacity + add_amount <= inventory_max_capacity;
    }

    public int GetCurrentCapacity()
    {
        return inventory_current_capacity;
    }
    public int GetCurrentMaxCapacity()
    {
        return inventory_max_capacity;
    }

    /**
     * Add items to inventory
     */
    public void AddItem(ItemType item, int amount)
    {
        if (items.ContainsKey(item))
        {
            items[item] += amount; // add amount
            inventory_current_capacity += amount *
                    item_manager.GetItem(item).item_size;

            OnInventoryChanged?.Invoke(item);
            return;
        }
        else
        {
            // item is not in inventory
            GrowInventory(item, amount);
        }
    }

    // utility for adding an item that is not in the inventory already
    private void GrowInventory(ItemType item, int amount)
    {
        items.Add(item, amount);
        inventory_current_capacity += amount *
                    item_manager.GetItem(item).item_size;
        OnInventoryChanged?.Invoke(item);
    }

    /**
     * RemoveItem returns the amount that was removed from the inventory, 0 if the item was not in the inventory.
     */
    public int RemoveItem(ItemType item, int amount)
    {
        // check if item is in inventory
        if (items.ContainsKey(item))
        {
            items[item] -= amount;
            if (items[item] < 0)
            {
                // amount is negative, calculate the difference and adjust amount removed
                int remove_amount = amount + items[item];
                items[item] = 0;
                inventory_current_capacity -= remove_amount *
                    item_manager.GetItem(item).item_size;

                OnInventoryChanged?.Invoke(item);
                return remove_amount;
            }
            inventory_current_capacity -= amount *
                    item_manager.GetItem(item).item_size;

            OnInventoryChanged?.Invoke(item);
            // on success return the amount we removed
            return amount;
        }
        else
        {
            // add to dictionary
            GrowInventory(item, 0);
            // did not remove anything since it was not in inventory
            return 0;
        }
    }

    public int GetItemAmount(ItemType item)
    {
        // attempt to find the amount of the given item
        return items.TryGetValue(item, out int amount) ? amount : 0;
    }

    /**
     * Produce items and consume the costs.
     */
    public void ProduceItems()
    {
        bool can_produce = true;
        int inventory_required = 0;
        foreach (InventorySlot cslot in consumes)
        {
            if (GetItemAmount(cslot.item) < cslot.amount)
            {
                // not enough of this item
                can_produce = false;
                break;
            }
            inventory_required -= cslot.amount *
                    item_manager.GetItem(cslot.item).item_size; // these items get removed
        }
        foreach (InventorySlot pslot in produces)
        {
            inventory_required += pslot.amount *
                    item_manager.GetItem(pslot.item).item_size; // these items get added
        }
        if (can_produce && inventory_required + inventory_current_capacity <= inventory_max_capacity)
        {
            // reduce items in inventory in quantity defined in consume set
            foreach (InventorySlot cslot in consumes)
            {
                // it costs amount items to produce X
                RemoveItem(cslot.item, cslot.amount);
            }
            // add items to inventory in quantity defined in produce set
            foreach (InventorySlot pslot in produces)
            {
                // produce X items, where X is amount in produces
                AddItem(pslot.item, pslot.amount);
            }
        }
    }

}

// used for adding default item to an inventory with the inspector
// also used for item production/consumption quantities.
[System.Serializable]
public class InventorySlot
{
    public ItemType item;
    public int amount; // quantity of items

    public InventorySlot(ItemType _item, int _amount)
    {
        item = _item;
        amount = _amount;
    }
}

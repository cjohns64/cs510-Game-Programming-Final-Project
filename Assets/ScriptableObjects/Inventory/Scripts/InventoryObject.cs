using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="New Inventory", menuName="Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    public InventorySlot[] container = new InventorySlot[(int) ItemType.GetNames(typeof(ItemType)).Length];
    public float credits = 0.0f;

    public void AddItem(ItemObject _item, int _amount)
    {
        foreach (InventorySlot slot in container)
        {
            if (slot.item == _item)
            {
                slot.AddAmount(_amount);
                return;
            }
        }
        // item is not in inventory
        container[(int) _item.type] = new InventorySlot(_item, _amount);
    }

    /**
     * RemoveItem returns the amount that was removed from the inventory, 0 if the item was not in the inventory.
     */
    public int RemoveItem(ItemObject _item, int _amount)
    {
        foreach (InventorySlot slot in container)
        {
            if (slot.item == _item)
            {
                // found item
                // max amount to remove cannot exceed what is in the inventory
                int remove_amount = slot.amount;
                // check if max allowed amount is more than we whant to remove
                if (remove_amount > _amount)
                {
                    // more items in inventory then we need
                    // remove_amount is the amount we will remove
                    remove_amount = _amount;
                }
                // only remove up to the amount actually in the inventory
                slot.RemoveAmount(remove_amount);
                // on success return the amount we removed
                return remove_amount;
            }
        }
        // item was not in Inventory
        return 0;
    }

    public ItemObject FindByType(ItemType item_type)
    {
        foreach (InventorySlot slot in container)
        {
            if (slot.item != null)
            {
                if (slot.item.type == item_type)
                {
                    // fount item return the ItemObject
                    return slot.item;
                }
            }
        }
        // did not find item
        return null;
    }

}

[System.Serializable]
public class InventorySlot
{
    public ItemObject item;
    public int amount;

    public InventorySlot(ItemObject _item, int _amount)
    {
        item = _item;
        amount = _amount;
    }

    public void AddAmount(int quantitiy)
    {
        amount += quantitiy;
    }

    public void RemoveAmount(int quantitiy)
    {
        amount -= quantitiy;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="New Inventory", menuName="Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    public List<InventorySlot> container = new List<InventorySlot>();
    public int credits;

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
        container.Add(new InventorySlot(_item, _amount));
    }

    /**
     * TODO check amount on return
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

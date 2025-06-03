using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private Inventory player_inventory;
    public void AddItems()
    {
        player_inventory.inventory_max_capacity = 99999999;
        foreach (ItemType type in ItemType.GetValues(typeof(ItemType)))
        {
            // save all planet items to local save manager
            player_inventory.AddItem(type, 500);
        }
    }
}

using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private Inventory player_inventory;
    public void AddItems()
    {
        player_inventory.inventory_bonus = 50000;
        foreach (ItemType type in ItemType.GetValues(typeof(ItemType)))
        {
            // save all planet items to local save manager
            player_inventory.AddItem(type, 500);
        }
    }
}

using UnityEngine;

[CreateAssetMenu(fileName="New Slot Upgrade", menuName="Inventory System/Items/Slot Upgrade")]
public class SlotUpgrade : ItemObject
{
    public float hull_bonus = 0.0f;
    public float shield_bonus = 0.0f;
    public float armor_bonus = 0.0f;
    public int cargo_bonus = 0;
    public float speed_bonus = 0.0f;
}

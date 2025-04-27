using UnityEngine;

[CreateAssetMenu(fileName="New Upgrade Object", menuName="Inventory System/Items/Upgrade")]
public class UpgradeObject : ItemObject
{
    public float hull_bonus = 0.0f;
    public float shield_bonus = 0.0f;
    public int cargo_bonus = 0;
}

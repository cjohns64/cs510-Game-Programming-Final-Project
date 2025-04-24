using UnityEngine;

[CreateAssetMenu(fileName="New Engine Object", menuName="Inventory System/Items/Engine")]
public class EngineObject : ItemObject
{
    public float engine_speed = 1.0f;

    public void Awake()
    {
        type = ItemType.Default;
    }
}

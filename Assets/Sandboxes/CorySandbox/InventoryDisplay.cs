using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryDisplay : MonoBehaviour
{
    public InventoryObject inventory;
    public int x_start;
    public int y_start;
    public int x_padding;
    public int y_padding;
    public int columns;
    Dictionary<InventorySlot, GameObject> itemsDisplayed = new Dictionary<InventorySlot, GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        int i=0;
        foreach (InventorySlot slot in inventory.container)
        {
            if (itemsDisplayed.ContainsKey(slot))
            {
                UpdateItemText(itemsDisplayed[slot], slot.amount, slot.item.item_name);
            }
            else
            {
                var obj = Instantiate(slot.item.prefab, Vector3.zero, Quaternion.identity, transform);
                obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
                UpdateItemText(obj, slot.amount, slot.item.item_name);
                itemsDisplayed.Add(slot, obj);
            }
            i++;
        }
    }

    public void CreateDisplay()
    {
        int i=0;
        foreach (InventorySlot slot in inventory.container)
        {
            var obj = Instantiate(slot.item.prefab, Vector3.zero, Quaternion.identity, transform);
            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
            UpdateItemText(obj, slot.amount, slot.item.item_name);
            itemsDisplayed.Add(slot, obj);
            i++;
        }
    }

    public Vector3 GetPosition(int i)
    {
        return new Vector3(x_start + (x_padding * (i % columns)), y_start + (-y_padding * (i / columns)), 0.0f);
    }

    public void UpdateItemText(GameObject _item, int _amount, string _name)
    {
        // get all child Components of type TextMeshProUGUI, these are the elements that will be updated
        TextMeshProUGUI[] components_list = _item.GetComponentsInChildren<TextMeshProUGUI>();

        // check each result, update the amount text and the item name
        foreach (TextMeshProUGUI ugui in components_list)
        {
            // set the amount element
            if (ugui.name == "ItemAmountText")
            {
                ugui.text = _amount.ToString("n0");
            }
            // set the name element
            else if (ugui.name == "ItemNameText")
            {
                ugui.text = _name;
            }
        }
    }
}

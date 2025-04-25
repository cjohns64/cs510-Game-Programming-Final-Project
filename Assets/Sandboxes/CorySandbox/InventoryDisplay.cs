using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryDisplay : MonoBehaviour
{
    public InventoryObject inventory;
    public bool show_price = false;
    Dictionary<InventorySlot, GameObject> itemsDisplayed = new Dictionary<InventorySlot, GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateDisplay();
    }

    // Update is called once per frame
    void FixedUpdate()
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
                UpdateItemText(itemsDisplayed[slot], slot);
            }
            else
            {
                AddItemUI(slot);
            }
            i++;
        }
    }

    public void CreateDisplay()
    {
        int i=0;
        foreach (InventorySlot slot in inventory.container)
        {
            AddItemUI(slot);
            i++;
        }
    }

    public void AddItemUI(InventorySlot _slot)
    {
        // skip null slots, items, and prefabs
        if (_slot.item != null)
        {
            var obj = Instantiate(_slot.item.prefab, transform);
            UpdateItemText(obj, _slot);
            itemsDisplayed.Add(_slot, obj);
        }
    }

    public void UpdateItemText(GameObject _item, InventorySlot _slot)
    {
        if (_slot.amount == 0) {
            // don't display this slot
            _item.SetActive(false);
        }
        else if (!_item.activeSelf)
        {
            // turn item back on if amount isn't 0
            _item.SetActive(true);
        }
        else
        {
            // get all child Components of type TextMeshProUGUI, these are the elements that will be updated
            TextMeshProUGUI[] components_list = _item.GetComponentsInChildren<TextMeshProUGUI>();

            // check each result, update the amount text and the item name
            foreach (TextMeshProUGUI ugui in components_list)
            {
                // set the amount element
                if (ugui.name == "ItemAmountText")
                {
                    ugui.text = _slot.amount.ToString("n0");
                }
                // set the name element
                else if (ugui.name == "ItemNameText")
                {
                    ugui.text = _slot.item.item_name;
                }
                // set the price element
                else if (show_price && ugui.name == "ItemPriceText")
                {
                    ugui.text = _slot.item.item_value.ToString("C2");
                }
            }
        }
    }
}

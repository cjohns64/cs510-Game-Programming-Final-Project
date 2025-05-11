using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using static UnityEditor.Progress;

/**
 * Inventory Display handles managing the user interface of a single inventory.
 * It is placed on the window the items are placed in.
 */
public class InventoryDisplay : MonoBehaviour
{
    // this is the inventory managed by this Inventory Display
    [SerializeField] private InventoryObject inventory;
    // The item manager that can translate the ItemType enum to the associated ItemObject.
    private ItemManager item_manager;
    public bool show_price = false;
    public bool is_player_inv = false;
    // internal dictionary of all items in the display, including disabled ones
    private Dictionary<ItemType, GameObject> itemsDisplayed = new();

    void Start()
    {
        // lookup the item manager
        item_manager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        //Debug.Log("Item manager :: " +  item_manager);
        // make sure the inventory is initialized
        inventory.InitInventory();
        // set up the display with all inventory objects, disable the ones with no items
        CreateDisplay();
    }

    public void UpdateDisplay(ItemType item)
    {
        // updates a single item in the display, call from an event
        if (itemsDisplayed.ContainsKey(item))
        {
            UpdateItemText(item, itemsDisplayed[item]);
        }
        else
        {
            AddItemUI(item);
        }
    }

    public void CreateDisplay()
    {
        // add an entry for each item
        foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
        {
            AddItemUI(item);
        }
    }
    public void UpdateItemText(ItemType item, GameObject game_object)
    {
        if (inventory.GetItemAmount(item) == 0) // no item
        {
            // don't display this slot
            game_object.SetActive(false);
        }
        else if (!game_object.activeSelf) // icon is disabled, already check that it isn't 0
        {
            // turn item back on if amount isn't 0
            game_object.SetActive(true);
        }
        else // amount != 0 and icon is active
        {
            // get all child Components of type TextMeshProUGUI, these are the elements that will be updated
            TextMeshProUGUI[] components_list = game_object.GetComponentsInChildren<TextMeshProUGUI>();

            // check each result, update the amount text and the item name
            foreach (TextMeshProUGUI ugui in components_list)
            {
                // set the amount element
                if (ugui.name == "ItemAmountText")
                {
                    ugui.text = inventory.GetItemAmount(item).ToString("n0");
                }
                // set the name element
                else if (ugui.name == "ItemNameText")
                {
                    ugui.text = item_manager.GetItem(item).item_name;
                }
                // set the price element
                else if (show_price && ugui.name == "ItemPriceText")
                {
                    ugui.text = item_manager.GetItem(item).item_value.ToString("C2");
                }
            }
        }
    }

    public void AddItemUI(ItemType item)
    {
        // add an item ui to the menu
        var obj = Instantiate(item_manager.item_ui_prefab, transform);
        // update item icon
        Image[] imageResults = obj.GetComponentsInChildren<Image>();
        // find the icon
        foreach (Image img in imageResults)
        {
            if (img.name == "ItemIcon")
            {
                // update the icon
                Debug.Log("adding item icon: " + item + " " + item_manager.GetItem(item));
                img.sprite = item_manager.GetItem(item).icon;
                break;
            }
        }
        // update the text
        UpdateItemText(item, obj);
        itemsDisplayed.Add(item, obj);
        }
    }
    

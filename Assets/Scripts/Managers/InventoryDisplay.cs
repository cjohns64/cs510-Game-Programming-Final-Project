using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/**
 * Inventory Display handles managing the user interface of a single inventory.
 * It is placed on the window the items are placed in.
 */
public class InventoryDisplay : MonoBehaviour
{
    // this is the inventory managed by this Inventory Display
    [SerializeField] private Inventory active_inventory;
    [SerializeField] private Inventory player_inventory;
    [SerializeField] private GameObject trade_menu;
    // The item manager that can translate the ItemType enum to the associated ItemObject.
    private ItemManager item_manager;
    private GameObject player_trade_area;
    private GameObject station_trade_area;
    public bool show_price = false;
    // internal dictionary of all items in the display, including disabled ones
    private Dictionary<ItemType, GameObject> player_items_displayed = new();
    private Dictionary<ItemType, GameObject> station_items_displayed = new();
    // scroll area update
    private int player_active_items = 0;
    private int station_active_items = 0;
    private RectTransform player_content_area;
    private RectTransform station_content_area;
    private float item_ui_height;

    void Start()
    {
        // lookup the item manager
        item_manager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        //Debug.Log("Item manager :: " +  item_manager);
        // lookup trade areas for player and stations
        player_trade_area = trade_menu.transform.Find("Viewport").Find("PlayerInventoryContent").gameObject;
        station_trade_area = trade_menu.transform.Find("Viewport").Find("s0-InventoryContent").gameObject;
        // calculate the height of the cargo ui prefab
        RectTransform cargo_item = item_manager.item_ui_prefab.GetComponent<RectTransform>();
        player_content_area = player_trade_area.GetComponent<RectTransform>();
        station_content_area = station_trade_area.GetComponent<RectTransform>();
        Vector3[] v = new Vector3[4];
        cargo_item.GetLocalCorners(v);
        item_ui_height = v[1].y - v[0].y + 5;

        // set up the display with all inventory objects, disable the ones with no items
        CreateDisplay(true); // player inventory display
        CreateDisplay(false); // default station inventory display
        // subscribe to the OnInventoryChanged Event for the default station inventory
        active_inventory.OnInventoryChanged += StationUpdateDisplay;
        // subscribe to the OnInventoryChanged Event for the player inventory
        player_inventory.OnInventoryChanged += (ItemType item) => { UpdateDisplay(item, true); };
    }

    private void StationUpdateDisplay(ItemType item)
    {
        UpdateDisplay(item, false);
    }

    public void SetNewActiveInventory(Inventory new_active_inventory)
    {
        // remove last station from subscriber pool
        active_inventory.OnInventoryChanged -= StationUpdateDisplay;
        // switch stations
        active_inventory = new_active_inventory;
        // subscribe to OnInventoryChanged
        active_inventory.OnInventoryChanged += StationUpdateDisplay;
    }

    public void UpdateAllItems(bool is_player_display)
    {
        foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
        {
            UpdateDisplay(item, is_player_display);
        }
    }

    public void UpdateDisplay(ItemType item, bool is_player_display)
    {
        // updates a single item in the display, call from an event
        if (is_player_display)
        {
            if (player_items_displayed.ContainsKey(item))
            {
                UpdateItemText(item, player_items_displayed[item], is_player_display);
            }
            else
            {
                AddItemUI(item, is_player_display);
            }
        }
        else
        {
            if (station_items_displayed.ContainsKey(item))
            {
                UpdateItemText(item, station_items_displayed[item], is_player_display);
            }
            else
            {
                AddItemUI(item, is_player_display);
            }
        }
        
    }

    private void CreateDisplay(bool is_player_display)
    {
        // add an entry for each item
        foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
        {
            AddItemUI(item, is_player_display);
        }
    }

    private void UpdateItemText(ItemType item, GameObject game_object, bool is_player_inventory)
    {
        bool test_item_amount = is_player_inventory ? 
            player_inventory.GetItemAmount(item) == 0 : active_inventory.GetItemAmount(item) == 0;

        if (test_item_amount) // no item
        {
            // don't display this slot
            game_object.SetActive(false);
            // update active items for scroll area
            if (is_player_inventory)
            {
                player_active_items--;
            }
            else
            {
                station_active_items--;
            }
        }
        else // amount != 0
        {
            // check if icon is disabled, we already checked that its amount isn't 0
            if (!game_object.activeSelf)
            {
                // turn item back on if amount isn't 0
                game_object.SetActive(true);
                // update active items for scroll area
                if (is_player_inventory)
                {
                    player_active_items++;
                }
                else
                {
                    station_active_items++;
                }
            }
            // get all child Components of type TextMeshProUGUI, these are the elements that will be updated
            TextMeshProUGUI[] components_list = game_object.GetComponentsInChildren<TextMeshProUGUI>();

            // check each result, update the amount text and the item name
            foreach (TextMeshProUGUI ugui in components_list)
            {
                // set the amount element
                if (ugui.name == "ItemAmountText")
                {
                    ugui.text = is_player_inventory ? 
                        player_inventory.GetItemAmount(item).ToString("n0") :
                        active_inventory.GetItemAmount(item).ToString("n0");
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
        if (is_player_inventory)
        {
            // update scroll area size
            player_content_area.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                item_ui_height * player_active_items);
            //Debug.Log(cargo_item_height * active_items);
        }
        else
        {
            // update scroll area size
            station_content_area.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                item_ui_height * station_active_items);
            //Debug.Log(cargo_item_height * active_items);
        }

    }

    private void AddItemUI(ItemType item, bool is_player_display)
    {
        // add an item ui to the menu
        var obj = Instantiate(item_manager.item_ui_prefab,
            is_player_display ? player_trade_area.transform : station_trade_area.transform);
        // update active items for scroll area
        if (is_player_display)
        {
            player_active_items++;
        }
        else
        {
            station_active_items++;
        }
        // set the inventory state on the button
        obj.GetComponentInChildren<ItemButton>().is_player_inv = is_player_display;
        // update item type
        var button_script = obj.GetComponentInChildren<ItemButton>();
        //Debug.Log("before " + button_script.thisItem);
        button_script.thisItem = item;
        //Debug.Log("after " + button_script.thisItem);
        // update item icon
        Image[] imageResults = obj.GetComponentsInChildren<Image>();
        // find the icon
        foreach (Image img in imageResults)
        {
            if (img.name == "ItemIcon")
            {
                // update the icon
                //Debug.Log("adding item icon: " + item + " " + item_manager.GetItem(item));
                img.sprite = item_manager.GetItem(item).icon;
                break;
            }
        }
        // update the text
        UpdateItemText(item, obj, is_player_display);
        if (is_player_display)
        {
            player_items_displayed.Add(item, obj);
            //Debug.Log("Player Display Add: " + item);
        }
        else
        {
            station_items_displayed.Add(item, obj);
            //Debug.Log("Station Display Add: " + item);
        }
    }
}
    

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
public class CargoDisplay : MonoBehaviour
{
    // this is the inventory managed by this Inventory Display
    [SerializeField] private Inventory player_inventory;
    [SerializeField] private GameObject cargo_ui_prefab;
    [SerializeField] private TextMeshProUGUI cargo_space_text;
    // The item manager that can translate the ItemType enum to the associated ItemObject.
    private ItemManager item_manager;
    private GameObject player_cargo_area;
    private RectTransform content_area;
    private float cargo_item_height;
    // internal dictionary of all items in the display, including disabled ones
    private Dictionary<ItemType, GameObject> player_items_displayed = new();
    private int active_items = 0;

    void Start()
    {
        // lookup the item manager
        item_manager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        //Debug.Log("Item manager :: " +  item_manager);
        // lookup cargo area for player
        player_cargo_area = this.transform.Find("Scroll View").Find("Viewport").Find("Content").gameObject;
        
        // calculate the height of the cargo ui prefab
        RectTransform cargo_item = cargo_ui_prefab.GetComponent<RectTransform>();
        content_area = player_cargo_area.GetComponent<RectTransform>();
        Vector3[] v = new Vector3[4];
        cargo_item.GetLocalCorners(v);
        cargo_item_height = v[1].y - v[0].y + 5;

        // set up the display with all inventory objects, disable the ones with no items
        CreateDisplay();
        // subscribe to the OnInventoryChanged Event for the player inventory
        player_inventory.OnInventoryChanged += (ItemType item) => { UpdateDisplay(item); };
        player_inventory.OnInventoryChanged += (ItemType item) => { UpdateCargoSpaceUI(); };
    }

    public void UpdateDisplay(ItemType item)
    {
        // updates a single item in the display, call from an event
        if (player_items_displayed.ContainsKey(item))
        {
            UpdateItemText(item, player_items_displayed[item], player_inventory);
        }
        else
        {
            AddItemUI(item);
        }
    }

    private void CreateDisplay()
    {
        // add an entry for each item
        foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
        {
            AddItemUI(item);
        }
        UpdateCargoSpaceUI();
    }

    private void UpdateItemText(ItemType item, GameObject game_object, Inventory inventory)
    {
        if (inventory.GetItemAmount(item) == 0) // no item
        {
            // don't display this slot
            game_object.SetActive(false);
            active_items--;
        }
        else // amount != 0
        {
            // check if icon is disabled, we already checked that its amount isn't 0
            if (!game_object.activeSelf)
            {
                // turn item back on if amount isn't 0
                game_object.SetActive(true);
                active_items++;
            }
            // get all child Components of type TextMeshProUGUI, these are the elements that will be updated
            TextMeshProUGUI[] components_list = game_object.GetComponentsInChildren<TextMeshProUGUI>();

            // check each result, update the amount text and the item name
            foreach (TextMeshProUGUI ugui in components_list)
            {
                // set the amount element
                if (ugui.name == "ItemQuantity")
                {
                    ugui.text = inventory.GetItemAmount(item).ToString("n0");
                }
                // set the name element
                else if (ugui.name == "ItemName")
                {
                    ugui.text = item_manager.GetItem(item).item_name;
                }
                // set the price element
                else if (ugui.name == "ItemValue")
                {
                    ugui.text = item_manager.GetItem(item).item_value.ToString("C2");
                }
                else if (ugui.name == "ItemDescription")
                {
                    ugui.text = item_manager.GetItem(item).description;
                }
            }
        }
        // update scroll area size
        content_area.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            cargo_item_height * active_items);
        //Debug.Log(cargo_item_height * active_items);
    }

    private void UpdateCargoSpaceUI()
    {
        cargo_space_text.text = player_inventory.GetCurrentCapacity().ToString("n0") +
                        "/" + player_inventory.GetCurrentMaxCapacity().ToString("n0");
    }

    private void AddItemUI(ItemType item)
    {
        // add an item ui to the menu
        var obj = Instantiate(cargo_ui_prefab, player_cargo_area.transform);
        active_items++;
        // update item icon
        Image[] imageResults = obj.GetComponentsInChildren<Image>();
        // find the icon
        foreach (Image img in imageResults)
        {
            if (img.name == "ItemImage")
            {
                // update the icon
                //Debug.Log("adding item icon: " + item + " " + item_manager.GetItem(item));
                img.sprite = item_manager.GetItem(item).icon;
                break;
            }
        }
        // update the text
        UpdateItemText(item, obj, player_inventory);
        player_items_displayed.Add(item, obj);
        //Debug.Log("Player Display Add: " + item);
    }
}
    

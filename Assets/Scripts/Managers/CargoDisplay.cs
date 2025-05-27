using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/**
 * Cargo Display manages the content of a single inventory display
 */
public class CargoDisplay : MonoBehaviour
{
    // this is the inventory managed by this script
    [SerializeField] private Inventory displayed_inventory;
    // this is the prefab that represents an item, it will be instanced for each item in the inventory
    [SerializeField] private GameObject cargo_ui_prefab;
    // this is the text object that displays the current inventory space
    [SerializeField] private TextMeshProUGUI cargo_space_text;
    // The item manager that can translate the ItemType enum to the associated ItemObject.
    private ItemManager item_manager;
    private GameObject cargo_area;
    private RectTransform content_area;
    private float cargo_item_height;
    // internal dictionary of all items in the display, including disabled ones
    private Dictionary<ItemType, GameObject> items_displayed = new();
    private int active_items = 0;
    private bool init = false;

    private void Start()
    {
        if (!init)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        // lookup the item manager
        item_manager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        // lookup cargo area for this object
        cargo_area = this.transform.Find("Scroll View").Find("Viewport").Find("Content").gameObject;
        Debug.Log(cargo_area.name);

        // calculate the height of the cargo ui prefab
        RectTransform cargo_item = cargo_ui_prefab.GetComponent<RectTransform>();
        content_area = cargo_area.GetComponent<RectTransform>();
        Vector3[] v = new Vector3[4];
        cargo_item.GetLocalCorners(v);
        cargo_item_height = v[1].y - v[0].y + 5;

        // set up the display with all inventory objects, disable the ones with no items
        CreateDisplay();
        SubscribeToEvents();
        init = true;
    }

    public void SwapInventories(Inventory inventory)
    {
        if (!init)
        {
            Initialize();
        }
        UnsubscribeFromEvents();
        displayed_inventory = inventory;
        SubscribeToEvents();
        active_items = CountActiveItems();
        UpdateAllItems();
    }

    private void InventoryChangedListener(ItemType item)
    {
        UpdateDisplay(item);
        UpdateCargoSpaceUI();
    }

    private void SubscribeToEvents()
    {
        // subscribe to the OnInventoryChanged Event for the displayed inventory
        displayed_inventory.OnInventoryChanged += InventoryChangedListener;
    }

    private void UnsubscribeFromEvents()
    {
        // subscribe to the OnInventoryChanged Event for the displayed inventory
        displayed_inventory.OnInventoryChanged -= InventoryChangedListener;
    }

    public void UpdateDisplay(ItemType item)
    {
        // updates a single item in the display, call from an event
        if (items_displayed.ContainsKey(item))
        {
            UpdateItemText(item, items_displayed[item], displayed_inventory);
        }
        else
        {
            AddItemUI(item);
        }
    }

    private void UpdateAllItems()
    {
        foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
        {
            UpdateDisplay(item);
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

    private int CountActiveItems()
    {
        int count = 0;
        foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
        {
            if (items_displayed.ContainsKey(item))
            {
                count++;
            }
        }
        return count;
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
        cargo_space_text.text = displayed_inventory.GetCurrentCapacity().ToString("n0") +
                        "/" + displayed_inventory.GetCurrentMaxCapacity().ToString("n0");
    }

    private void AddItemUI(ItemType item)
    {
        // add an item ui to the menu
        var obj = Instantiate(cargo_ui_prefab, cargo_area.transform);
        active_items++;
        // update item icon
        Image[] imageResults = obj.GetComponentsInChildren<Image>();
        // update item type
        var button_script = obj.GetComponentInChildren<ItemButton>();
        //Debug.Log("before " + button_script.thisItem);
        button_script.thisItem = item;
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
        UpdateItemText(item, obj, displayed_inventory);
        items_displayed.Add(item, obj);
        //Debug.Log("Player Display Add: " + item);
    }

    public Inventory GetInventoryForTrading()
    {
        return displayed_inventory;
    }
}
    

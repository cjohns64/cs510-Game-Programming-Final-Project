using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.Events;

/**
 * Manages: station inventories, trading, item production, supply and demand calculations.
 */
public class TradeManager : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private GameObject trade_menu; // needed because trade menu starts inactive
    [SerializeField] private string global_scripts = "GlobalScripts";
    private InventoryDisplay inventoryDisplay;
    private Inventory current_station_inventory;
    private ItemManager itemManager;
    private TextMeshProUGUI playerCreditsText;
    private TextMeshProUGUI stationCreditsText;
    private TMP_InputField quantityValueText;

    // production cycle event
    public UnityEvent OnProductionCycle;
    public UnityEvent OnTradeFailNotEnoughFunds;
    public UnityEvent OnTradeFailNotEnoughSpace;
    // time delay between production cycles
    private float production_timer = 0.0f;
    private float production_delay = 30.0f;

    public OrbitMoverAnalytic playerMover;
    public event Action<CelestialBody> OnMenuClosed;
    private CelestialBody currentBody;

    private void Awake()
    {
        Initialize();
    }

    private void FixedUpdate()
    {
        // every trading cycle, trigger item production
        production_timer += Time.deltaTime;
        if (production_timer > production_delay )
        {
            OnProductionCycle.Invoke();
            production_timer = 0.0f;
        }
    }

    private void Initialize()
    {
        // lookup all components needed by the trade manager
        // lookup item manager
        itemManager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        // lookup the global scripts manager and find the inventory display component
        inventoryDisplay = GameObject.Find(global_scripts).GetComponent<InventoryDisplay>();
        // lookup player credits text area
        playerCreditsText = trade_menu.transform.Find("CreditsPanel").Find("PlayerCreditsValueText").gameObject.GetComponent<TextMeshProUGUI>();
        //Debug.Log("player credits text=" + playerCreditsText.name);
        // lookup station credits text area
        stationCreditsText = trade_menu.transform.Find("CreditsPanel").Find("StationCreditsValueText").gameObject.GetComponent<TextMeshProUGUI>();
        // Debug.Log("station credits text=" + stationCreditsText.name);
        // lookup quantity text area
        quantityValueText = trade_menu.transform.Find("QuantityUI").Find("QuantityInputField").gameObject.GetComponent<TMP_InputField>();
    }

    void SwitchStations()
    {
        if (current_station_inventory != null)
        {
            // unsubscribe old inventory
            current_station_inventory.OnInventoryChanged -= event_listener_UpdateCreditsText;
        }
        // swap inventories
        current_station_inventory = currentBody.GetComponentInParent<Inventory>();
        // subscribe new inventory
        current_station_inventory.OnInventoryChanged += event_listener_UpdateCreditsText;
        // swap station inventory in inventory display
        inventoryDisplay.active_inventory = current_station_inventory;
        // update all items in trade area
        inventoryDisplay.UpdateAllItems(false);
        inventoryDisplay.UpdateAllItems(true);
    }

    float GetItemCost(ItemType item)
    {
        //TODO add supply and demand calculations
        //Debug.Log("" + item==ItemType.Metals + " " + itemManager.name);
        return itemManager.GetItem(item).item_value;
    }

    /**
     * Process a click event from an item in an inventory.
     */
    public void ClickedOn(ItemType item_type, bool is_player_inv)
    {
        // get the amount
        int quantity = int.Parse(quantityValueText.text);
        // determine the other inventory
        if (is_player_inv)
        {
            // other inventory is a station inventory
            //Debug.Log("player inventory clicked, quantity = " + quantity);
            TradeItems(item_type, playerInventory, current_station_inventory, quantity);
        }
        else
        {
            // other inventory is the player inventory
            //Debug.Log("station inventory clicked, quantity = " + quantity);
            TradeItems(item_type, current_station_inventory, playerInventory, quantity);
        }
    }
    public void event_listener_UpdateCreditsText(ItemType x)
    {
        UpdateCreditsText();
    }

    public void UpdateCreditsText()
    {
        // update player credits text
        playerCreditsText.text = playerInventory.credits.ToString("c2");
        // update station credits text
        stationCreditsText.text = current_station_inventory.credits.ToString("c2");
    }

    public void TradeItems(ItemType item_from, Inventory inventory_from, Inventory inventory_to, int amount)
    {
        float credits_buyer = inventory_to.credits;
        float unit_cost = GetItemCost(item_from);
        float full_cost = amount * unit_cost;
        // allow transaction if full cost can be paid for
        if (full_cost > inventory_to.credits) 
        {
            // didn't have enough funds
            OnTradeFailNotEnoughFunds.Invoke();
        }
        else if (!inventory_to.HasCapacity(amount))
        {
            // didn't have enough inventory space
            OnTradeFailNotEnoughSpace.Invoke();
        }
        else
        {
            // remove items from seller inventory
            int sold_quantity = inventory_from.RemoveItem(item_from, amount);
            // add item to buyer inventory
            inventory_to.AddItem(item_from, sold_quantity);
            // add credits to seller inventory
            inventory_from.credits += (sold_quantity * unit_cost);
            // remove credits form buyer inventory
            inventory_to.credits -= (sold_quantity * unit_cost);
            // update ui
            UpdateCreditsText();
        }
    }

    public void OpenMenu(CelestialBody body) {
        currentBody = body;
        // set the new body's inventory and trade area
        SwitchStations();
        // activate the menu
        trade_menu.SetActive(true);
    }

    /// <summary>
    /// Call this from a "Close" or "Done" button in the UI.
    /// </summary>
    public void CloseMenu() {
        trade_menu.SetActive(false);
        OnMenuClosed?.Invoke(currentBody);
    }
}

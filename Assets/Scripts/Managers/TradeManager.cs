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
    [SerializeField] private GameObject ship;
    [SerializeField] private GameObject trade_menu; // needed because trade menu starts inactive
    [SerializeField] private GameObject player_menu;
    private PlayerMenuTabManager tab_manager;
    [SerializeField] private string global_scripts = "GlobalScripts";
    private InventoryDisplay inventoryDisplay;
    private Inventory current_station_inventory;
    private Inventory playerInventory;
    private ItemManager itemManager;
    private TextMeshProUGUI playerCreditsText;
    private TextMeshProUGUI stationCreditsText;
    private TextMeshProUGUI player_cargo_text;
    private TextMeshProUGUI station_cargo_text;
    private TMP_InputField quantityValueText;

    // production cycle event
    public UnityEvent OnProductionCycle;
    public UnityEvent OnTradeFailNotEnoughFunds;
    public UnityEvent OnTradeFailNotEnoughSpace;
    public event Action<CelestialBody> OnMenuClosed;
    // time delay between production cycles
    private float production_timer = 0.0f;
    private float production_delay = 30.0f;

    //private OrbitMoverAnalytic playerMover;
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
        // lookup tab manager script from player menu
        tab_manager = player_menu.GetComponent<PlayerMenuTabManager>();
        // lookup all components needed by the trade manager
        playerInventory = ship.GetComponent<Inventory>();
        //playerMover = ship.GetComponent<OrbitMoverAnalytic>();
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
        // lookup cargo space text areas
        station_cargo_text = trade_menu.transform.Find("CreditsPanel").Find("StationCargo").gameObject.GetComponent<TextMeshProUGUI>();
        player_cargo_text = trade_menu.transform.Find("CreditsPanel").Find("PlayerCargo").gameObject.GetComponent<TextMeshProUGUI>();
        // lookup quantity text area
        quantityValueText = trade_menu.transform.Find("QuantityUI").Find("QuantityInputField").gameObject.GetComponent<TMP_InputField>();
    }

    public void SwitchStations()
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
        inventoryDisplay.SetNewActiveInventory(current_station_inventory);
        // update all items in trade area
        inventoryDisplay.UpdateAllItems(false);
        inventoryDisplay.UpdateAllItems(true);
    }

    public float GetItemCost(ItemType item)
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
        if (current_station_inventory != null)
        {
            // update station credits text
            stationCreditsText.text = current_station_inventory.credits.ToString("c2");
            // update inventory space text
            station_cargo_text.text = current_station_inventory.GetCurrentCapacity().ToString("n0") +
                "/" + current_station_inventory.GetCurrentMaxCapacity().ToString("n0");
        }
        // update player credits text
        playerCreditsText.text = playerInventory.credits.ToString("c2");
        // update inventory space text
        player_cargo_text.text = playerInventory.GetCurrentCapacity().ToString("n0") +
            "/" + playerInventory.GetCurrentMaxCapacity().ToString("n0");
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
        tab_manager.ActivateTradeTab();
    }

    /// <summary>
    /// Call this from a "Close" or "Done" button in the UI.
    /// </summary>
    public void CloseMenu() {
        player_menu.SetActive(false);
        OnMenuClosed?.Invoke(currentBody);
    }
}

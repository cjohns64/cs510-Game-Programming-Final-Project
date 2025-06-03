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
    [Header("Required Resources")]
    [SerializeField] private GameObject player_menu;
    [SerializeField] private CargoDisplay player_cargo_display;
    [SerializeField] private CargoDisplay docked_cargo_display;
    [SerializeField] private TMP_InputField player_quantity_text;
    [SerializeField] private TMP_InputField station_quantity_text;

    // resolved resources, these will be looked up during initialization
    private PlayerMenuTabManager tab_manager;
    private ItemManager itemManager;

    // production cycle events
    public UnityEvent OnProductionCycle;
    public UnityEvent OnTradeFailNotEnoughSpace;
    public event Action<CelestialBody> OnMenuClosed;
    // time delay between production cycles
    private float production_timer = 0.0f;
    private readonly float production_delay = 30.0f;

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
        // lookup item manager
        itemManager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
    }

    public void SwitchStations()
    {
        if (docked_cargo_display != null && currentBody != null && currentBody.GetComponentInParent<Inventory>() != null)
        {
            // swap inventories
            docked_cargo_display.SwapInventories(currentBody.GetComponentInParent<Inventory>());
        }
    }

    public float GetItemCost(ItemType item)
    {
        //Debug.Log("" + item==ItemType.Metals + " " + itemManager.name);
        //return itemManager.GetItem(item).item_value;
        return 0f; // cost is disabled
    }

    /**
     * Process a click event from an item in an inventory.
     */
    public void ClickedOn(ItemType item_type, bool is_player_inv)
    {
        // only allow item trading if docked
        if (tab_manager.IsDocked()) 
        {
            // get the amount
            int quantity = int.Parse(is_player_inv ? player_quantity_text.text : station_quantity_text.text);
            // determine the other inventory
            if (is_player_inv)
            {
                // other inventory is a station inventory
                //Debug.Log("player inventory clicked, quantity = " + quantity);
                TradeItems(item_type,
                    player_cargo_display.GetInventoryForTrading(),
                    docked_cargo_display.GetInventoryForTrading(),
                    quantity);
            }
            else
            {
                // other inventory is the player inventory
                //Debug.Log("station inventory clicked, quantity = " + quantity);
                TradeItems(item_type,
                    docked_cargo_display.GetInventoryForTrading(),
                    player_cargo_display.GetInventoryForTrading(),
                    quantity);
            }
        }
    }

    public void TradeItems(ItemType item_from, Inventory inventory_from, Inventory inventory_to, int amount)
    {
        if (!inventory_to.HasCapacity(amount * itemManager.GetItem(item_from).item_size))
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
        }
    }

    public void OpenMenu(CelestialBody body) {
        currentBody = body;
        // set the new body's inventory and trade area
        SwitchStations();
        // activate the menu
        tab_manager.ActivateTradeTab();
    }

    
    // called from PlayerMenuTabManager
    public void InvokeOnMenuClosedForCurrentBody() {
        OnMenuClosed?.Invoke(currentBody);
    }
}

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
    [SerializeField] private InventoryObject playerInventory;
    [SerializeField] private List<InventoryObject> tradingPosts = new();
    [SerializeField] private GameObject trade_menu; // needed because trade menu starts inactive
    private List<GameObject> station_trade_areas = new();
    private ItemManager itemManager;
    private TextMeshProUGUI playerCreditsText;
    private TextMeshProUGUI stationCreditsText;
    private TMP_InputField quantityValueText;
    private bool is_initialized = false;
    private int currentStation = 0;
    // the number in station trade area name == active station number
    private Regex regex = new Regex(@"\d+");
    // production cycle event
    public UnityEvent OnProductionCycle;
    public UnityEvent OnTradeFailNotEnoughFunds;
    public UnityEvent OnTradeFailNotEnoughSpace;
    // time delay between production cycles
    private float production_timer = 0.0f;
    private float production_delay = 5.0f;

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
        // lookup all station trade areas, the exact number is not known
        Transform tmp = trade_menu.transform.Find("Viewport").transform.Find("s0-InventoryContent");
        station_trade_areas.Add(tmp.gameObject);
        int i = 0;
        while (tmp != null)
        {
            i++;
            // find the next station
            tmp = trade_menu.transform.Find("Viewport").transform.Find("s" + i + "-InventoryContent");
            if (tmp != null)
            {
                // set it to inactive
                tmp.gameObject.SetActive(false);
                station_trade_areas.Add(tmp.gameObject);
            }
        }
        // lookup player credits text area
        playerCreditsText = trade_menu.transform.Find("CreditsPanel").Find("PlayerCreditsValueText").gameObject.GetComponent<TextMeshProUGUI>();
        //Debug.Log("player credits text=" + playerCreditsText.name);
        // lookup station credits text area
        stationCreditsText = trade_menu.transform.Find("CreditsPanel").Find("StationCreditsValueText").gameObject.GetComponent<TextMeshProUGUI>();
        // Debug.Log("station credits text=" + stationCreditsText.name);
        // lookup quantity text area
        quantityValueText = trade_menu.transform.Find("QuantityUI").Find("QuantityInputField").gameObject.GetComponent<TMP_InputField>();
        // Debug.Log("quantity value text=" + quantityValueText.name);
        is_initialized = true;
        // subscribe to inventory update events
        foreach (InventoryObject inv in tradingPosts)
        {
            inv.OnInventoryChanged += event_listener_UpdateCreditsText;
        }

        
    }

    void OnEnable ()
    {
        if (!is_initialized)
        {
            Initialize();
        }
        // update ui
        UpdateCreditsText();
        // update active station trade area
        foreach (GameObject station in station_trade_areas)
        {
            Match match = regex.Match(station.name);
            if (match.Success)
            {
                // set all station trade areas to inactive except the current one
                station.SetActive(currentStation == int.Parse(match.Value));
            }
        }
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
            TradeItems(item_type, playerInventory, tradingPosts[currentStation], quantity);
        }
        else
        {
            // other inventory is the player inventory
            //Debug.Log("station inventory clicked, quantity = " + quantity);
            TradeItems(item_type, tradingPosts[currentStation], playerInventory, quantity);
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
        stationCreditsText.text = tradingPosts[currentStation].credits.ToString("c2");
    }

    public void TradeItems(ItemType item_from, InventoryObject inventory_from, InventoryObject inventory_to, int amount)
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
        trade_menu.SetActive(true);
        UpdateCreditsText();
    }

    /// <summary>
    /// Call this from a "Close" or "Done" button in the UI.
    /// </summary>
    public void CloseMenu() {
        trade_menu.SetActive(false);
        OnMenuClosed?.Invoke(currentBody);
    }
}

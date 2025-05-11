using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/**
 * Manages: station inventories, trading, item production, supply and demand calculations.
 */
public class TradeManager : MonoBehaviour
{
    [SerializeField] private InventoryObject playerInventory;
    [SerializeField] private List<InventoryObject> tradingPosts = new List<InventoryObject>();
    private ItemManager itemManager;
    private GameObject playerTradeArea;
    private GameObject stationTradeArea;
    private TextMeshProUGUI playerCreditsText;
    private TextMeshProUGUI stationCreditsText;
    private TMP_InputField quantityValueText;
    private bool is_initialized = false;
    private int currentStation = 0;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        // lookup all components needed by the trade manager
        // lookup item manager
        itemManager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        // most are children of the Trade Menu
        GameObject trade_menu = GameObject.Find("Trade Menu");
        // Debug.Log("trade_menu=" + trade_menu.name);
        // lookup player trade area. It is a child of trade_menu, so use transform.Find
        playerTradeArea = trade_menu.transform.Find("Viewport").Find("PlayerInventoryContent").gameObject;
        // Debug.Log("player trade area=" + playerTradeArea.name);
        // lookup station trade area
        stationTradeArea = trade_menu.transform.Find("Viewport").Find("StationInventoryContent").gameObject;
        // Debug.Log("station trade area=" + stationTradeArea.name);
        // lookup player credits text area
        playerCreditsText = trade_menu.transform.Find("CreditsPanel").Find("PlayerCreditsValueText").gameObject.GetComponent<TextMeshProUGUI>();
        Debug.Log("player credits text=" + playerCreditsText.name);
        // lookup station credits text area
        stationCreditsText = trade_menu.transform.Find("CreditsPanel").Find("StationCreditsValueText").gameObject.GetComponent<TextMeshProUGUI>();
        // Debug.Log("station credits text=" + stationCreditsText.name);
        // lookup quantity text area
        quantityValueText = trade_menu.transform.Find("QuantityUI").Find("QuantityInputField").gameObject.GetComponent<TMP_InputField>();
        // Debug.Log("quantity value text=" + quantityValueText.name);
        is_initialized = true;
    }

    void OnEnable ()
    {
        if (!is_initialized)
        {
            Initialize();
        }
        // update player credits text
        playerCreditsText.text = playerInventory.credits.ToString("c2");
        // update station credits text
        stationCreditsText.text = tradingPosts[currentStation].credits.ToString("c2");
    }

    float GetItemCost(ItemType item)
    {
        //TODO add supply and demand calculations
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
            Debug.Log("player inventory clicked, quantity = " + quantity);
            TradeItems(item_type, playerInventory, tradingPosts[currentStation], quantity);
        }
        else
        {
            // other inventory is the player inventory
            Debug.Log("station inventory clicked, quantity = " + quantity);
            TradeItems(item_type, tradingPosts[currentStation], playerInventory, quantity);
        }
    }

    public void TradeItems(ItemType item_from, InventoryObject inventory_from, InventoryObject inventory_to, int amount)
    {
        float credits_buyer = inventory_to.credits;
        float unit_cost = GetItemCost(item_from);
        float full_cost = amount * unit_cost;
        // allow transaction if full cost can be paid for
        if (full_cost <= inventory_to.credits)
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
            // update player credits text
            playerCreditsText.text = playerInventory.credits.ToString("c2");
            // update station credits text
            stationCreditsText.text = tradingPosts[currentStation].credits.ToString("c2");
        }
    }
}

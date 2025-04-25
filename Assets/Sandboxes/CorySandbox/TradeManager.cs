using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/**
 *
 * Manages: station inventories, trading, item production, supply and demand calculations.
 *
 */
public class TradeManager : MonoBehaviour
{
    public InventoryObject playerInventory;
    public List<Station> stations = new List<Station>();
    public InventoryObject stationTESTInventory;
    public GameObject playerTradeArea;
    public GameObject stationTradeArea;
    public TextMeshProUGUI playerCreditsText;
    public TextMeshProUGUI stationCreditsText;
    public TMP_InputField quantityValueText;

    private int currentStation = 0;

    void Awake()
    {
        stations.Add(new Station(stationTESTInventory));
    }

    void OnEnable ()
    {
        // update player credits text
        playerCreditsText.text = playerInventory.credits.ToString("c2");
        // update station credits text
        stationCreditsText.text = stations[currentStation].stationInventory.credits.ToString("c2");
    }

    float GetItemCost(ItemObject _item)
    {
        //TODO add supply and demand calculations
        return _item.item_value;
    }

    public void ClickedOn(ItemType _item_type, InventoryObject _inventory_containing)
    {
        // get the ItemObject connected with the ItemType
        ItemObject item_clicked = _inventory_containing.FindByType(_item_type);
        // check for null item
        if (item_clicked)
        {
            // get the amount
            int quantitiy = int.Parse(quantityValueText.text);
            // determine the other inventory
            if (_inventory_containing == playerInventory)
            {
                // other inventory is a station inventory
                Debug.Log("player inventory clicked, quantitiy = " + quantitiy);
                TradeItems(item_clicked, _inventory_containing, stations[currentStation].stationInventory, quantitiy);
            }
            else
            {
                // other inventory is the player inventory
                Debug.Log("station inventory clicked, quantitiy = " + quantitiy);
                TradeItems(item_clicked, _inventory_containing, playerInventory, quantitiy);
            }
        }
        else
        {
            Debug.Log("Clicked Item was not found in inventory!");
        }
    }

    public void TradeItems(ItemObject _item_from, InventoryObject _inventory_from, InventoryObject _inventory_to, int amount)
    {
        float credits_buyer = _inventory_to.credits;
        float unit_cost = GetItemCost(_item_from);
        float full_cost = amount * unit_cost;
        // allow transaction if full cost can be paid for
        if (full_cost <= _inventory_to.credits)
        {
            // remove items from seller inventory
            int sold_quantity = _inventory_from.RemoveItem(_item_from, amount);
            // add item to buyer inventory
            _inventory_to.AddItem(_item_from, sold_quantity);
            // add credits to seller inventory
            _inventory_from.credits += (sold_quantity * unit_cost);
            // remove credits form buyer inventory
            _inventory_to.credits -= (sold_quantity * unit_cost);
            // update ui
            // update player credits text
            playerCreditsText.text = playerInventory.credits.ToString("c2");
            // update station credits text
            stationCreditsText.text = stations[currentStation].stationInventory.credits.ToString("c2");
        }
    }
}


public class Station
{
    public InventoryObject stationInventory;

    public Station(InventoryObject inventory)
    {
        stationInventory = inventory;
    }
}

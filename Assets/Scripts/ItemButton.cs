using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    public ItemType thisItem;
    private TradeManager tradeManager;
    private InventoryObject parentInventory;
    [SerializeField] private Button btn = null;

    // some code from https://stackoverflow.com/questions/69259615/how-to-detect-if-button-is-clicked-unity
    private void Awake()
    {
        // get a reference to the Trade Manager in the Global Scripts GameObject
        tradeManager = GameObject.Find("GlobalScripts").GetComponent<TradeManager>();
        // get a reference to the parent's inventory
        parentInventory = GetComponentInParent<InventoryDisplay>().inventory;
        // adding a delegate with no parameters
        btn.onClick.AddListener(OnClick);
    }
    
    private void OnClick()
    {
        if (tradeManager && parentInventory)
        {
            tradeManager.ClickedOn(thisItem, parentInventory);
        }
        else
        {
            Debug.Log("Button clicked but setup failed");
        }

    }
}

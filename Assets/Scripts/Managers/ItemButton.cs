using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    public ItemType thisItem;
    [SerializeField] private string global_scripts = "GlobalScripts";
    private TradeManager tradeManager;
    private InventoryDisplay parentInventoryDisplay;
    [SerializeField] private Button btn = null;

    // some code from https://stackoverflow.com/questions/69259615/how-to-detect-if-button-is-clicked-unity
    private void Awake()
    {
        // get a reference to the Trade Manager in the Global Scripts GameObject
        tradeManager = GameObject.Find(global_scripts).GetComponent<TradeManager>();
        // get a reference to the parent's inventory
        parentInventoryDisplay = GetComponentInParent<InventoryDisplay>();
        // adding a delegate with no parameters
        btn.onClick.AddListener(OnClick);
    }
    
    private void OnClick()
    {
        if (tradeManager && parentInventoryDisplay)
        {
            tradeManager.ClickedOn(thisItem, parentInventoryDisplay.is_player_inv);
        }
        else
        {
            Debug.Log("Button clicked but setup failed");
        }
    }
}

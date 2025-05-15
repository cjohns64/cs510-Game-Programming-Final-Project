using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    public ItemType thisItem;
    public bool is_player_inv = false;
    [SerializeField] private string global_scripts = "GlobalScripts";
    private TradeManager tradeManager;
    [SerializeField] private Button btn = null;

    // some code from https://stackoverflow.com/questions/69259615/how-to-detect-if-button-is-clicked-unity
    private void Awake()
    {
        // get a reference to the Trade Manager in the Global Scripts GameObject
        tradeManager = GameObject.Find(global_scripts).GetComponent<TradeManager>();
        
        // adding a delegate with no parameters
        btn.onClick.AddListener(OnClick);
    }
    
    private void OnClick()
    {
        if (tradeManager)
        {
            tradeManager.ClickedOn(thisItem, is_player_inv);
        }
        else
        {
            Debug.Log("Button clicked but setup failed");
        }
    }
}

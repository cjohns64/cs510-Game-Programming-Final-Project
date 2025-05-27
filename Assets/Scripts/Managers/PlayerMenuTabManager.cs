using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuTabManager : MonoBehaviour
{
    [SerializeField] private TimeController timeController;

    [Header("Tab control buttons")]
    [SerializeField] private Button objectives_tab_button;
    [SerializeField] private Button trade_tab_button;
    [SerializeField] private Button cargo_tab_button;
    [SerializeField] private Button upgrades_tab_button;

    [Header("Tab game objects")]
    [SerializeField] private GameObject objectives_tab;
    [SerializeField] private GameObject trade_tab;
    [SerializeField] private GameObject cargo_tab;
    [SerializeField] private GameObject upgrades_tab;

    [Header("Default settings")]
    [SerializeField] private string global_scripts = "GlobalScripts";
    // internal references
    private TradeManager trade_manager;
    private UpgradeManager upgrade_manager;
    private bool is_docked = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        upgrade_manager = GameObject.Find(global_scripts).GetComponent<UpgradeManager>();
        trade_manager = GameObject.Find(global_scripts).GetComponent<TradeManager>();
    }

    public bool IsDocked()
    {
        return is_docked;
    }

    /**
     * Resets the state of all buttons and their tabs to the unfocused state
     * Individual activate functions can call this one first, then write their states over these ones.
     */
    private void ClearAllTabStates()
    {
        // ensure this menu is active
        this.gameObject.SetActive(true);
        // disable all tabs
        objectives_tab.SetActive(false);
        trade_tab.SetActive(false);
        cargo_tab.SetActive(false);
        upgrades_tab.SetActive(false);
        // set all buttons to interactable
        objectives_tab_button.interactable = true;
        trade_tab_button.interactable = is_docked; // trade menu should only be interactable if the player is docked
        cargo_tab_button.interactable = true;
        upgrades_tab_button.interactable = true;
    }
    public void ActivateObjectivesTab()
    {
        ClearAllTabStates();
        // activate this tab, deactivate this button
        objectives_tab.SetActive(true);
        objectives_tab_button.interactable = false;
    }

    public void ActivateTradeTab()
    {
        ClearAllTabStates();
        is_docked = true;
        trade_tab.SetActive(true);
        trade_tab_button.interactable = false;
        
    }

    public void ActivateCargoTab()
    {
        ClearAllTabStates();
        cargo_tab.SetActive(true);
        cargo_tab_button.interactable = false;
    }

    public void ActivateUpgradeTab()
    {
        ClearAllTabStates();
        upgrades_tab.SetActive(true);
        upgrades_tab_button.interactable = false;
        upgrade_manager.CheckInventory();
    }

    public void OpenMenu()
    {
        ActivateObjectivesTab();
        // Pause time
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Call this from a "Close" or "Done" button in the UI.
    /// </summary>
    public void CloseMenu()
    {
        if (is_docked)
        {
            // undock
            trade_manager.InvokeOnMenuClosedForCurrentBody(); // call from here so CloseMenu can be setup from within the prefab
            is_docked = false;
        }
        // resume time
        timeController.SetTimeScale();
        this.gameObject.SetActive(false);
    }
}

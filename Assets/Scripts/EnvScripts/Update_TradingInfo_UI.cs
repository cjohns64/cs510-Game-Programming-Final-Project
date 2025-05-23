using UnityEngine;
using TMPro;

public class Update_TradingInfo_UI : MonoBehaviour
{
    private Inventory inventory;
    public string station_name;
    private TextMeshProUGUI name_text;
    private TextMeshProUGUI credits_text;
    private TextMeshProUGUI cargo_text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // lookup parent of parent's Inventory component
        inventory = this.GetComponentInParent<Inventory>();
        // lookup child ui elements
        name_text = this.transform.Find("NameText").gameObject.GetComponent<TextMeshProUGUI>();
        name_text.text = station_name;
        credits_text = this.transform.Find("CreditsText").gameObject.GetComponent<TextMeshProUGUI>();
        cargo_text = this.transform.Find("CargoText").gameObject.GetComponent<TextMeshProUGUI>();

        UpdateUI(ItemType.AntiGravGenerator); // update ui, parameter is not used
        inventory.OnInventoryChanged += UpdateUI; // subscribe to inventory changed event
    }

    public void UpdateUI(ItemType x) // x is not used, but is required by existing event
    {
        if (inventory != null)
        {
            credits_text.text = inventory.credits.ToString("C2");
            cargo_text.text = inventory.GetCurrentCapacity().ToString() + "/" + inventory.GetCurrentMaxCapacity().ToString();
        }
    }
}

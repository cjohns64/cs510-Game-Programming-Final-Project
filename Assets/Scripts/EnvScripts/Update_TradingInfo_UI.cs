using UnityEngine;
using TMPro;

public class Update_TradingInfo_UI : MonoBehaviour
{
    [SerializeField] private InventoryObject inventory;
    public string station_name;
    private TextMeshProUGUI name_text;
    private TextMeshProUGUI credits_text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // lookup child ui elements
        name_text = this.transform.Find("NameText").gameObject.GetComponent<TextMeshProUGUI>();
        name_text.text = station_name;
        credits_text = this.transform.Find("CreditsText").gameObject.GetComponent<TextMeshProUGUI>();
        UpdateUI(ItemType.AntiGravGenerator); // update ui, parameter is not used
        inventory.OnInventoryChanged += UpdateUI; // subscribe to inventory changed event
    }

    public void UpdateUI(ItemType x) // x is not used, but is required by existing event
    {
        if (inventory != null)
        {
            credits_text.text = inventory.credits.ToString("C2");
        }
    }
}

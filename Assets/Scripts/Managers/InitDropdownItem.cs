using UnityEngine;
using UnityEngine.UI;

public class InitDropdownItem : MonoBehaviour
{
    // controller allows the type of the dropdown to be set outside of the dropdown items
    [SerializeField] private DropdownController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // set the interactable state based off of the TODO manager
        // lookup the Toggle component
        Toggle toggle_component = GetComponent<Toggle>();
        if (toggle_component != null)
        {
            // get this item's index from its name
            // "Item 0: Solar Sails"
            // only works up to item 9, but dropdowns in the upgrade menu only go up to 5
            int index = this.gameObject.name[5] - '0';
            //Debug.Log(this.gameObject.name + " " + index);
            toggle_component.interactable = controller.GetInteractableState(index);
        }
    }
}

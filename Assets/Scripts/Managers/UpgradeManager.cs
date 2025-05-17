using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq;

public enum DropdownSelector {
    EngineSelector,
    Slot0Selector,
    Slot1Selector,
    Slot2Selector,
    Slot3Selector,
    Slot4Selector,
    Slot5Selector,
    Slot6Selector,
    Slot7Selector
}

//https://docs.unity3d.com/2018.3/Documentation/ScriptReference/UI.Dropdown.html
public class UpgradeManager : MonoBehaviour
{
    // ui data
    [SerializeField] private GameObject upgrade_menu;
    [SerializeField] private GameObject ship;
    private Inventory player_inventory;
    private List<TMP_Dropdown> expansion_dropdowns = new();
    private ItemType[] expansion_types = { ItemType.HullBrace, ItemType.HullExtenderM1,
        ItemType.OuterStabilizers, ItemType.EngineArmSmall, ItemType.HullExtenderM2,
        ItemType.InnerStabilizers, ItemType.EngineArmLarge
    };
    private ItemType[] engine_types = { ItemType.SolarSails, ItemType.ChemicalEngine,
        ItemType.IonEngine, ItemType.NuclearThermalEngine, ItemType.WarpEngine,
        ItemType.WormholeEngine
    };
    private ItemType[] slot_types = { ItemType.ArmorModule, ItemType.ShieldingModule,
        ItemType.CargoModule
    };
    private List<TMP_Dropdown> slot_dropdowns = new();
    private TMP_Dropdown engine_dropdown;
    private int[] last_slot_values = Enumerable.Repeat(0, 8).ToArray();
    private int last_engine_value = 0;
    private bool[] engine_dropdown_options = { false, false, false, false, false, false };
    private bool[,] slot_dropdown_options = { { false, false, false, false }, { false, false, false, false },
        { false, false, false, false }, { false, false, false, false }, { false, false, false, false },
        { false, false, false, false }, { false, false, false, false }, { false, false, false, false }
    };
    // mesh data
    private List<GameObject> expansion_meshes = new();
    private List<GameObject> armor_meshes = new();
    private List<GameObject> cargo_meshes = new();
    private List<GameObject> shield_meshes = new();
    private List<GameObject> engine_mesh_parents = new();

    void Start()
    {
        // lookup the ship mesh
        GameObject ship_mesh = ship.transform.Find("ShipModel").gameObject;
        // lookup the player's inventory, it is attached to the parent of the ship mesh
        player_inventory = ship.GetComponent<Inventory>();

        // lookup Viewport
        GameObject ui_parent = upgrade_menu.transform.Find("Viewport").gameObject;
        // lookup engine dropdown menu
        engine_dropdown = ui_parent.transform.Find("EngineDropdown").gameObject.GetComponent<TMP_Dropdown>();
        // lookup ship expansion dropdown menus
        GameObject exp_ui_parent = ui_parent.transform.Find("expansion-selection").gameObject;
        for (int i=1; i<8; i++)
        {
            // expansion dropdowns are named in the form 0#-ShipExpansionDropdown
            expansion_dropdowns.Add(exp_ui_parent.transform.Find(
                "0" + i.ToString() + "-ShipExpansionDropdown"
                ).gameObject.GetComponent<TMP_Dropdown>());
        }

        // lookup slot dropdown menus
        GameObject slot_ui_parent = ui_parent.transform.Find("slot-selection").gameObject;
        for (int i = 0; i < 8; i++)
        {
            // slot dropdowns are named in the form SlotUpgradeDropdown_#
            slot_dropdowns.Add(slot_ui_parent.transform.Find(
                "SlotUpgradeDropdown_" + i.ToString()).gameObject.GetComponent<TMP_Dropdown>());
            last_slot_values[i] = slot_dropdowns[i].value; // update last value
        }

        // lookup ship expansion mesh sections
        GameObject exp_parent = ship_mesh.transform.Find("Ship-Expansions").gameObject;
        for (int i=1; i<8; i++)
        {
            // expansion components are organized into empties named E#
            expansion_meshes.Add(exp_parent.transform.Find("E" + i.ToString()).gameObject);
        }

        // lookup ship upgrade slot meshes
        GameObject slot_parent = ship_mesh.transform.Find("Upgrade-Slots").gameObject;
        // armor slots
        for (int i=0; i<8; i++)
        {
            // slot upgrades are named in the pattern type_#, the baseship (0) is also included
            armor_meshes.Add(slot_parent.transform.Find("Armor_" + i.ToString()).gameObject);
        }
        // cargo slots
        for (int i = 0; i < 8; i++)
        {
            // slot upgrades are named in the pattern type_#, the baseship (0) is also included
            cargo_meshes.Add(slot_parent.transform.Find("Cargo_" + i.ToString()).gameObject);
        }
        // shielding slots
        for (int i = 0; i < 8; i++)
        {
            // slot upgrades are named in the pattern type_#, the baseship (0) is also included
            shield_meshes.Add(slot_parent.transform.Find("Shields_" + i.ToString()).gameObject);
        }

        // lookup engine upgrades
        // try and lookup tiers 0-5
        for (int i=0; i<6;  i++)
        {
            Transform tmp = ship_mesh.transform.Find("Engines").Find("T" + i.ToString());
            if (tmp != null)
            {
                // found tier
                engine_mesh_parents.Add(tmp.gameObject);
            }
        }
        last_engine_value = engine_dropdown.value;

        // add onValueChanged event listeners
        foreach (TMP_Dropdown d in expansion_dropdowns)
        {
            d.onValueChanged.AddListener(delegate {
                SelectionTriggeredShipUpdate(d);
            });
        }
        foreach (TMP_Dropdown d in slot_dropdowns)
        {
            d.onValueChanged.AddListener(delegate {
                SelectionTriggeredShipUpdate(d);
            });
        }
        engine_dropdown.onValueChanged.AddListener(delegate {
            SelectionTriggeredShipUpdate(engine_dropdown);
        });
    }

    /**
     * Checks the given inventory for upgrade items and enables associated options
     * Will only disable options if their dropdown is set to option 0,
     * since an enabled option indicates an item in use.
     * 
     * Must be triggered by a unity event when the upgrade menu is opened
     */
    public void CheckInventory()
    {
        CheckInventorySlotItems();
        CheckInventoryExpansionItems();
        CheckInventoryEngineItems();
    }

    /**
     * Checks the current state of the inventory for slot upgrade items.
     * Updates availability of dropdown options for all slot upgrade dropdown menus.
     */
    private void CheckInventorySlotItems()
    {
        // slot modules types + empty
        int[] slot_amounts = Enumerable.Repeat(0, slot_types.Length + 1).ToArray();
        for (int i = 0; i < slot_amounts.Length; i++)
        {
            if (i == 0)
            {
                // empty slot entry
                slot_amounts[i] = 1;
            }
            else
            {
                // 3 slot upgrade types
                slot_amounts[i] = player_inventory.GetItemAmount(slot_types[i - 1]);
            }
        }
        // slot modules
        for (int i = 0; i < slot_dropdowns.Count; i++)
        {
            // NOTE: slot_dropdowns.Count == slot_dropdown_options.GetLength(0)
            for (int j = 0; j < slot_dropdown_options.GetLength(1); j++)
            {
                if (j == 0)
                {
                    // empty option is always valid
                    slot_dropdown_options[i, j] = true;
                }
                // i = slot dropdown index, j-1 = upgrade index, j==0 is a empty slot
                else if (slot_amounts[j] > 0)
                {
                    // enable slot option
                    slot_dropdown_options[i, j] = true;
                }
                else if (slot_dropdowns[i].value != j)
                {
                    // already checked that j!=0, slot_amounts[j] > 0
                    // disable slot option on all dropdowns with a value not equal to j,
                    // these are slots that are in use
                    slot_dropdown_options[i, j] = false;
                }
            }
        }
    }

    /**
    * Checks the current state of the inventory for Engine items.
    * Updates dropdown options in the engine dropdown menu.
    */
    private void CheckInventoryEngineItems()
    {
        // engines
        int[] engine_amounts = Enumerable.Repeat(0, engine_types.Length).ToArray();
        for (int i = 0; i < engine_amounts.Length; i++)
        {
            engine_amounts[i] = player_inventory.GetItemAmount(engine_types[i]);
        }
        // engines
        for (int i = 0; i < engine_amounts.Length; i++)
        {
            if (engine_amounts[i] > 0)
            {
                // enable associated dropdown option
                engine_dropdown_options[i] = true;
            }
            else if (engine_dropdown.value != i && engine_amounts[i] == 0)
            {
                // disable option, since there are none in inventory and the dropdown has a different selection
                engine_dropdown_options[i] = false;
            }
        }
    }

    /**
    * Checks the current state of the inventory for ship expansion upgrade items.
    * Updates expansion dropdown availability.
    */
    private void CheckInventoryExpansionItems()
    {
        // ship expansions
        int[] exp_amounts = Enumerable.Repeat(0, expansion_types.Length).ToArray();
        for (int i = 0; i < exp_amounts.Length; i++)
        {
            // empty value does not need to be added since expansions enable/disable the entire dropdown
            // instead of options in the dropdown items
            exp_amounts[i] = player_inventory.GetItemAmount(expansion_types[i]);
        }
        // ship expansions
        for (int i = 0; i < exp_amounts.Length; i++)
        {
            //Debug.Log("check amounts " + i + " " +  exp_amounts[i] + " " + expansion_types[i].ToString());
            if (exp_amounts[i] > 0)
            {
                // enable associated dropdown
                expansion_dropdowns[i].gameObject.SetActive(true);
            }
            else if (expansion_dropdowns[i].value == 0)
            {
                // this expansion is no longer valid
                expansion_dropdowns[i].gameObject.SetActive(false);
            }
            // skip the case of no item in inventory and dropdown value is not 0
        }
    }

    /**
     * Dropdown Items in upgrade menu will call this via their controller script when they are instanced.
     * They will then set their interactable state with the return value.
     */
    public bool GetInteractableState(int index, DropdownSelector type)
    {
        switch (type)
        {
            case DropdownSelector.EngineSelector:
                return engine_dropdown_options[index];
            case DropdownSelector.Slot0Selector:
                return slot_dropdown_options[0, index];
            case DropdownSelector.Slot1Selector:
                return slot_dropdown_options[1, index];
            case DropdownSelector.Slot2Selector:
                return slot_dropdown_options[2, index];
            case DropdownSelector.Slot3Selector:
                return slot_dropdown_options[3, index];
            case DropdownSelector.Slot4Selector:
                return slot_dropdown_options[4, index];
            case DropdownSelector.Slot5Selector:
                return slot_dropdown_options[5, index];
            case DropdownSelector.Slot6Selector:
                return slot_dropdown_options[6, index];
            case DropdownSelector.Slot7Selector:
                return slot_dropdown_options[7, index];
            default: return true;
        }
    }

    private void SelectionTriggeredShipUpdate(TMP_Dropdown d)
    {
        int value = d.value;
        string slot_name = d.name;
        //Debug.Log("Updating Mesh " +  slot_name + " " + value);
        if (slot_name[0] == 'E')
        {
            // engine slot
            // Add/Remove item from inventory
            EngineInventoryUpdate(value);
            // Update mesh
            UpdateEngineMesh(value);
        }
        else if (slot_name[0] == 'S')
        {
            // upgrade slot, "SlotUpgradeDropdown_#"
            int slot_number = slot_name[slot_name.Length - 1] - '0';
            // Add/Remove item from inventory
            SlotInventoryUpdate(value, slot_number);
            // Update mesh
            UpdateSlotMesh(value, slot_number);
        }
        else
        {
            // expansion slot, "0#-ShipExpansionDropdown"
            int slot_number = slot_name[1] - '0';
            // Add/Remove item from inventory
            ExpansionInventoryUpdate(value, slot_number);
            // Update mesh
            UpdateExpansionMesh(value, slot_number);
        }
    }

    private void UpdateEngineMesh(int selection)
    {
        for (int i = 0; i < engine_mesh_parents.Count; i++)
        {
            // only the engine with the given index will be active
            engine_mesh_parents[i].SetActive(i == selection);

            // also set extra engine meshes, if expansions are active
            // check expansion module 4
            if (expansion_dropdowns[4-1].value == 1) // if the expansion is not active then the extra engines won't be either
            {
                Transform test = engine_mesh_parents[i].transform.Find("Ex4");
                if (test != null)
                {
                    // enable/disable extra engine meshes
                    test.gameObject.SetActive(selection == i);
                }
            }
            // check expansion module 7
            if (expansion_dropdowns[7-1].value == 1) // if the expansion is not active then the extra engines won't be either
            {
                Transform test = engine_mesh_parents[i].transform.Find("Ex7");
                if (test != null)
                {
                    // enable/disable extra engine meshes
                    test.gameObject.SetActive(selection == i);
                }
            }
        }
    }
    private void UpdateSlotMesh(int selection, int slot)
    {
        armor_meshes[slot].SetActive(selection == 1);
        shield_meshes[slot].SetActive(selection == 2);
        cargo_meshes[slot].SetActive(selection == 3);
    }

    /**
     * Add/Remove the correct engine item from the inventory
     */
    private void EngineInventoryUpdate(int selection)
    {
        // add last item to inventory
        player_inventory.AddItem(engine_types[last_engine_value], 1);
        // remove selection from inventory
        player_inventory.RemoveItem(engine_types[selection], 1);
        // update last item
        last_engine_value = selection;
        // inventory changed
        CheckInventoryEngineItems();
    }

    /**
     * Add/Remove the correct slot upgrade item from the inventory
     */
    private void SlotInventoryUpdate(int selection, int slot)
    {
        bool changed_inv = false;
        if (last_slot_values[slot] > 0)
        {
            // add last item to inventory, since it wasn't empty
            // slot type ranges from 0-2, last slot values ranges from 0-3
            player_inventory.AddItem(slot_types[last_slot_values[slot] - 1], 1);
            changed_inv = true;
        }
        if (selection > 0)
        {
            // remove new selection from inventory, since it is not empty
            player_inventory.RemoveItem(slot_types[selection - 1], 1);
            changed_inv = true;
        }
        // update last item
        last_slot_values[slot] = selection;

        if (changed_inv)
        {
            // inventory item changed
            CheckInventorySlotItems();
        }
    }

    /**
     * Add/Remove the correct expansion item from the inventory
     */
    private void ExpansionInventoryUpdate(int selection, int slot)
    {
        if (selection == 0)
        {
            // removing upgrade, add to player inventory
            player_inventory.AddItem(expansion_types[slot - 1], 1);
        }
        else
        {
            // adding upgrade, remove from player inventory
            player_inventory.RemoveItem(expansion_types[slot - 1], 1);
        }
    }

    private void UpdateExpansionMesh(int selection, int slot)
    {
        // numbered 1-7, but indexes are 0-6
        expansion_meshes[slot - 1].SetActive(selection == 1);
        // update dropdown availability
        // slot 0 is baseship and not affected by expansions
        slot_dropdowns[slot].gameObject.SetActive(selection == 1);
        if (selection == 0)
        {
            // removing expansion
            slot_dropdowns[slot].value = 0; // remove upgrade
        }
        if (slot == 4 || slot == 7)
        {
            // these slots also have engine components
            Transform test = engine_mesh_parents[engine_dropdown.value].transform.Find("Ex" + slot.ToString());
            if (test != null)
            {
                // enable/disable extra engine meshes
                test.gameObject.SetActive(selection == 1);
            }
        }
    }
}
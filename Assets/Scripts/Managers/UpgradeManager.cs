using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

//https://docs.unity3d.com/2018.3/Documentation/ScriptReference/UI.Dropdown.html
public class UpgradeManager : MonoBehaviour
{
    // ui data
    [SerializeField] private GameObject upgrade_menu;
    [SerializeField] private InventoryObject player_inventory;
    private List<TMP_Dropdown> expansion_dropdowns = new();
    private ItemType[] expansion_types = { ItemType.HullBrace, ItemType.HullExtenderM1,
        ItemType.OuterStabilizers, ItemType.EngineArmSmall, ItemType.HullExtenderM2,
        ItemType.InnerStabilizers, ItemType.EngineArmLarge
    };
    private List<TMP_Dropdown> slot_dropdowns = new();
    private TMP_Dropdown engine_dropdown;
    // mesh data
    [SerializeField] private GameObject ship_mesh;
    private List<GameObject> expansion_meshes = new();
    private List<GameObject> armor_meshes = new();
    private List<GameObject> cargo_meshes = new();
    private List<GameObject> shield_meshes = new();
    private List<GameObject> engine_mesh_parents = new();

    void Start()
    {
        player_inventory.InitInventory();
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

        // add onValueChanged event listeners
        foreach (TMP_Dropdown d in expansion_dropdowns)
        {
            d.onValueChanged.AddListener(delegate {
                UpdateMesh(d);
            });
        }
        foreach (TMP_Dropdown d in slot_dropdowns)
        {
            d.onValueChanged.AddListener(delegate {
                UpdateMesh(d);
            });
        }
        engine_dropdown.onValueChanged.AddListener(delegate {
            UpdateMesh(engine_dropdown);
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
        int[] exp_amounts = { 0, 0, 0, 0, 0, 0, 0 };
        // get amounts
        for (int i=0; i<7; i++)
        {
            exp_amounts[i] = player_inventory.GetItemAmount(expansion_types[i]);
        }
       
        // check amounts
        for (int i=0; i<7; i++)
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

    void UpdateMesh(TMP_Dropdown d)
    {
        int value = d.value;
        string slot_name = d.name;
        //Debug.Log("Updating Mesh " +  slot_name + " " + value);
        if (slot_name[0] == 'E')
        {
            // engine slot
            UpdateEngineMesh(value);
            
        }
        else if (slot_name[0] == 'S')
        {
            // upgrade slot, "SlotUpgradeDropdown_#"
            int slot_number = slot_name[slot_name.Length - 1] - '0';
            UpdateSlotMesh(value, slot_number);
        }
        else
        {
            // expansion slot, "0#-ShipExpansionDropdown"
            int slot_number = slot_name[1] - '0';
            UpdateExpansionMesh(value, slot_number);
        }
    }

    void UpdateEngineMesh(int selection)
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
    void UpdateSlotMesh(int selection, int slot)
    {
        armor_meshes[slot].SetActive(selection == 1);
        shield_meshes[slot].SetActive(selection == 2);
        cargo_meshes[slot].SetActive(selection == 3);
    }
    void UpdateExpansionMesh(int selection, int slot)
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
        // Add/Remove item from inventory
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
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

//https://docs.unity3d.com/2018.3/Documentation/ScriptReference/UI.Dropdown.html
public class UpgradeManager : MonoBehaviour
{
    public TMP_Dropdown slot1;
    public TMP_Dropdown slot2;
    public TMP_Dropdown slot3;
    public TMP_Dropdown slot4;
    public TMP_Dropdown slot5;
    public TMP_Dropdown slot6;
    public TMP_Dropdown engineSlot;
    public GameObject slot1_Armor_Mesh;
    public GameObject slot1_Shield_Mesh;
    public GameObject slot1_Cargo_Mesh;
    public GameObject slot2_Armor_Mesh;
    public GameObject slot2_Shield_Mesh;
    public GameObject slot2_Cargo_Mesh;
    public GameObject slot3_Armor_Mesh;
    public GameObject slot3_Shield_Mesh;
    public GameObject slot3_Cargo_Mesh;
    public GameObject slot4_Armor_Mesh;
    public GameObject slot4_Shield_Mesh;
    public GameObject slot4_Cargo_Mesh;
    public GameObject slot5_Armor_Mesh;
    public GameObject slot5_Shield_Mesh;
    public GameObject slot5_Cargo_Mesh;
    public GameObject slot6_Armor_Mesh;
    public GameObject slot6_Shield_Mesh;
    public GameObject slot6_Cargo_Mesh;
    public GameObject engineSlot_tier0;
    public GameObject engineSlot_tier1;
    public GameObject engineSlot_tier2;
    public GameObject[,] slot_mesh_array = new GameObject[6,4];
    // private GameObject[,] slot_mesh_array = {{null, slot1_Armor_Mesh, slot1_Shield_Mesh, slot1_Cargo_Mesh},
    //                                         {null, slot2_Armor_Mesh, slot2_Shield_Mesh, slot2_Cargo_Mesh},
    //                                         {null, slot3_Armor_Mesh, slot3_Shield_Mesh, slot3_Cargo_Mesh},
    //                                         {null, slot4_Armor_Mesh, slot4_Shield_Mesh, slot4_Cargo_Mesh},
    //                                         {null, slot5_Armor_Mesh, slot5_Shield_Mesh, slot5_Cargo_Mesh},
    //                                         {null, slot6_Armor_Mesh, slot6_Shield_Mesh, slot6_Cargo_Mesh}
    // };
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // must be initialized after slot vars
        slot_mesh_array[0, 0] = null;
        slot_mesh_array[0, 1] = slot1_Armor_Mesh;
        slot_mesh_array[0, 2] = slot1_Shield_Mesh;
        slot_mesh_array[0, 3] = slot1_Cargo_Mesh;
        slot_mesh_array[1, 0] = null;
        slot_mesh_array[1, 1] = slot2_Armor_Mesh;
        slot_mesh_array[1, 2] = slot2_Shield_Mesh;
        slot_mesh_array[1, 3] = slot2_Cargo_Mesh;
        slot_mesh_array[2, 0] = null;
        slot_mesh_array[2, 1] = slot3_Armor_Mesh;
        slot_mesh_array[2, 2] = slot3_Shield_Mesh;
        slot_mesh_array[2, 3] = slot3_Cargo_Mesh;
        slot_mesh_array[3, 0] = null;
        slot_mesh_array[3, 1] = slot4_Armor_Mesh;
        slot_mesh_array[3, 2] = slot4_Shield_Mesh;
        slot_mesh_array[3, 3] = slot4_Cargo_Mesh;
        slot_mesh_array[4, 0] = null;
        slot_mesh_array[4, 1] = slot5_Armor_Mesh;
        slot_mesh_array[4, 2] = slot5_Shield_Mesh;
        slot_mesh_array[4, 3] = slot5_Cargo_Mesh;
        slot_mesh_array[5, 0] = null;
        slot_mesh_array[5, 1] = slot6_Armor_Mesh;
        slot_mesh_array[5, 2] = slot6_Shield_Mesh;
        slot_mesh_array[5, 3] = slot6_Cargo_Mesh;

        int s1 = slot1.value;
        slot1.onValueChanged.AddListener(delegate {UpdateMesh(slot1);});
        int s2 = slot2.value;
        slot2.onValueChanged.AddListener(delegate {UpdateMesh(slot2);});
        int s3 = slot3.value;
        slot3.onValueChanged.AddListener(delegate {UpdateMesh(slot3);});
        int s4 = slot4.value;
        slot4.onValueChanged.AddListener(delegate {UpdateMesh(slot4);});
        int s5 = slot5.value;
        slot5.onValueChanged.AddListener(delegate {UpdateMesh(slot5);});
        int s6 = slot6.value;
        slot6.onValueChanged.AddListener(delegate {UpdateMesh(slot6);});
        int engine = engineSlot.value;
        engineSlot.onValueChanged.AddListener(delegate {UpdateMesh(engineSlot);});
        // Debug.Log("s1= " + s1 + ", s2= " + s2 + ", s3= " + s3 + ", s4= " + s4+ ", s5= " + s5+ ", s6= " + s6 + ", engine= " + engine);
    }

    private void SlotSetActiveMesh(int slot_number, int selection_index)
    {
        for (int i=0; i<slot_mesh_array.GetLength(0); i++)
        {
            // only activate the selection index mesh
            if (slot_mesh_array[slot_number, i])
            {
                // skip null objects. This is for when the selection is empty
                slot_mesh_array[slot_number, i].SetActive(i==selection_index);
            }
        }
    }

    void UpdateMesh(TMP_Dropdown dropdown)
    {
        int newValue = dropdown.value;
        string slotName = dropdown.gameObject.name;
        switch (slotName[4])
        {
            case '1':
                // slot 1 changed. The gameObject name will be in the form Slot#_Menu, or EngineDropdown
                SlotSetActiveMesh(0, newValue);
                break;
            case '2':
                // slot 2 changed.
                SlotSetActiveMesh(1, newValue);
                break;
            case '3':
                // slot 3 changed.
                SlotSetActiveMesh(2, newValue);
                break;
            case '4':
                // slot 4 changed.
                SlotSetActiveMesh(3, newValue);
                break;
            case '5':
                // slot 5 changed.
                SlotSetActiveMesh(4, newValue);
                break;
            case '6':
                // slot 6 changed.
                SlotSetActiveMesh(5, newValue);
                break;
            case 'n':
                // engine slot changed.
                engineSlot_tier0.SetActive(newValue==0);
                engineSlot_tier1.SetActive(newValue==1);
                engineSlot_tier2.SetActive(newValue==2);
                break;
        }
        Debug.Log("Value changed " + dropdown.value.ToString() + " menu: " + dropdown.gameObject);
    }
}

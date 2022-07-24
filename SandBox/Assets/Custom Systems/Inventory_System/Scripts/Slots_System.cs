using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slots_System : MonoBehaviour
{
    private Slots_DataBase dataBase;
    public Slot[] slots;

    private void Awake()
    {
        dataBase = GameObject.FindGameObjectWithTag("GameController").GetComponent<Slots_DataBase>();
    }

    public void Connect_toDataBase()
    {
        dataBase.Connect_Slots_System(this);
    }

    // out
    public void Move_Slot_to_StaticSlotsSystem(Item_Info itemInfo, int amount)
    {
        dataBase.Move_SlotsSystem_to_StaticSlotsSystem(itemInfo, amount);
    }
    // in
    public void Assign_to_EmptySlot(Item_Info itemInfo, int amount)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            // stack check

            if (slots[i] == null)
            {
                slots[i].Assign_Slot(itemInfo, amount);
            }
        }
    }
}
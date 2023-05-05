using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ItemEff/Consumable/Health")]
public class ItemHealingEff : ItemEffect
{
    public int healingPoint = 0;

    public override bool ExecuteRole()
    {
        Debug.Log("heal");
        return true;
    }
}

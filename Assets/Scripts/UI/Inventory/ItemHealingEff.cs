using UnityEngine;

namespace Hypocrites.UI.Inventory
{
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
}
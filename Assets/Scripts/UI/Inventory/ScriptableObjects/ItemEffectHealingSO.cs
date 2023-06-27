using UnityEngine;

namespace Hypocrites.UI.Inventory
{
    using DB.Data;

    [CreateAssetMenu(menuName = "Item/Consumable/Health")]
    public class ItemEffectHealingSO : ItemEffectBaseSO
    {
        public int healingPoint = 0;

        public override bool Execute(PlayerData target)
        {
            if (target == null)
            {
                Debug.Log($"{healingPoint} heald");
                return true;
            }

            target.Healed(healingPoint);

            return true;
        }
    }
}
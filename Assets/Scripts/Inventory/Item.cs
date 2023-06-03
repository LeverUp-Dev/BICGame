using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.Inventory
{
    public enum ItemType
    {
        Equipment,
        Artifact,
        Consumable
    }

    [System.Serializable]
    public class Item
    {
        public ItemType itemType;
        public string itemName;
        public Sprite itemImage;
        public List<ItemEffect> effs;

        public bool Use()
        {
            bool isUsed = false;

            foreach (ItemEffect eff in effs)
                isUsed = eff.ExecuteRole();

            return isUsed;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Hypocrites
{
    using Defines;
    using DB.Save;
    using UI.Inventory;
    using DB.Data;

    [Serializable]
    public class ItemData
    {
        [field: SerializeField] public ItemType ItemType { get; private set; }
        [field: SerializeField] public string ItemName { get; private set; }
        [field: SerializeField] public string ItemImagePath { get; private set; }
        [field: SerializeField] public string[] ItemEffectPaths { get; private set; }

        [field: SerializeField] public Sprite ItemImage { get; private set; }
        [field: SerializeField] public ItemEffectBaseSO[] ItemEffects { get; private set; }

        public ItemData(ItemSave save)
        {
            Load(save);
        }

        void Load(ItemSave save)
        {
            ItemType = save.itemType;
            ItemName = save.itemName;
            ItemImagePath = save.itemImagePath;
            ItemEffectPaths = save.itemEffectPaths;

            ItemImage = Resources.Load<Sprite>(ItemImagePath);
            ItemEffects = new ItemEffectBaseSO[ItemEffectPaths.Length];

            for (int i = 0; i < ItemEffectPaths.Length; i++)
            {
                ItemEffects[i] = Resources.Load<ItemEffectBaseSO>(ItemEffectPaths[i]);
            }
        }

        public bool Use(PlayerData target)
        {
            for (int i = 0; i < ItemEffects.Length; i++)
            {
                ItemEffects[i].Execute(target);
            }

            return true;
        }
    }
}

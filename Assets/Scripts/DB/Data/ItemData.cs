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

            ItemImage = Resources.Load<Sprite>(ItemImagePath);

            int effectsCount = save.itemEffectPaths.Length;
            ItemEffects = new ItemEffectBaseSO[effectsCount];
            for (int i = 0; i < effectsCount; i++)
                ItemEffects[i] = Resources.Load<ItemEffectBaseSO>(save.itemEffectPaths[i]);
        }

        public bool Use(Member target)
        {
            foreach (var effect in ItemEffects)
                effect.Execute(target);

            return true;
        }
    }
}

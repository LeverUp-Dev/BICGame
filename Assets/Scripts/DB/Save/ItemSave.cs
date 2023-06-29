using System;

namespace Hypocrites.DB.Save
{
    using Defines;

    [Serializable]
    public class ItemSave
    {
        public ItemType itemType;
        public string itemName;
        public string itemImagePath;
        public string[] itemEffectPaths;
    }
}

using UnityEngine;

namespace Hypocrites.UI.Inventory
{
    public class FieldItems : MonoBehaviour
    {
        public ItemData item;
        public SpriteRenderer image;

        public void SetItem(ItemData _item)
        {
            item = _item;

            image.sprite = _item.ItemImage;
        }

        public ItemData GetItem()
        {
            return item;
        }

        public void DestoryItem()
        {
            Destroy(gameObject);
        }
    }
}

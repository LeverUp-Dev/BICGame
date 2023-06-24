using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hypocrites.UI.Inventory
{
    public class Slot : MonoBehaviour, IPointerUpHandler
    {
        public int slotNum;
        public Item item;
        public Image itemIcon;

        public void UpdateSlotUI()
        {
            itemIcon.sprite = item.itemImage;
            itemIcon.gameObject.SetActive(true);
        }

        public void RemoveSlot()
        {
            item = null;
            itemIcon.gameObject.SetActive(false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            bool isUse = item.Use();
            if (isUse)
            {
                Inventory.Instance.RemoveItem(item.itemType, slotNum);
            }
        }
    }
}
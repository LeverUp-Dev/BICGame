using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hypocrites.UI.Inventory
{
    public class Slot : MonoBehaviour, IPointerUpHandler
    {
        public int slotNum;
        public ItemData item;
        public Image itemIcon;

        public void UpdateSlotUI()
        {
            itemIcon.sprite = item.ItemImage;
            itemIcon.gameObject.SetActive(true);
        }

        public void RemoveSlot()
        {
            item = null;
            itemIcon.gameObject.SetActive(false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (item != null)
            {
                // ���� ������ ���� �� � �÷��̾�(�Ǵ� ����)���� ����� ���� ������� ����
                if (item.Use(null))
                {
                    Inventory.Instance.RemoveItem(item.ItemType, slotNum);
                }
            }
        }
    }
}
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
                // 추후 아이템 선택 시 어떤 플레이어(또는 동료)에게 사용할 건지 물어보도록 수정
                if (item.Use(null))
                {
                    Inventory.Instance.RemoveItem(item.ItemType, slotNum);
                }
            }
        }
    }
}
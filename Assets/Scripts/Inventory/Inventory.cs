using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.Inventory
{
    public class Inventory : MonoBehaviour
    {
        #region Singleton
        public static Inventory instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }
        #endregion

        public delegate void OnSlotCountChange(int value);
        public OnSlotCountChange onSlotCountChange;

        public delegate void OnGetItem();
        public OnGetItem onGetItem;

        public delegate void OnChangeTab(int _index);
        public OnChangeTab onChangeTab;

        public int curTab = 0;
        public List<Item>[] itemTabs = new List<Item>[3] { new List<Item>(), new List<Item>(), new List<Item>() };

        private int slotCount;
        public int SlotCount
        {
            get => slotCount;
            set
            {
                slotCount = value;
                onSlotCountChange(slotCount);
            }
        }

        public List<Item> GetCurrentTabItems()
        {
            return itemTabs[curTab];
        }

        public void ChangeTab(int _index)
        {
            curTab = _index;
            onChangeTab(_index);
        }

        public bool AddItem(Item _item)
        {
            List<Item> items = itemTabs[(int)_item.itemType];
            if (items.Count < SlotCount)
            {
                items.Add(_item);
                onGetItem?.Invoke();
                return true;
            }

            return false;
        }

        public void RemoveItem(ItemType _type, int _index)
        {
            List<Item> items = itemTabs[(int)_type];
            items.RemoveAt(_index);
            onGetItem();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("FieldItem"))
            {
                FieldItems fieldItems = collision.GetComponent<FieldItems>();
                if (AddItem(fieldItems.GetItem()))
                {
                    fieldItems.DestoryItem();
                }
            }
        }
    }
}
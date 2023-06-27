using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.UI.Inventory
{
    using Defines;

    public class Inventory : MonoBehaviour
    {
        #region Singleton
        public static Inventory Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            // ¾À ÀüÈ¯ ½Ã¿¡ ½Ì±ÛÅæ °´Ã¼°¡ ÆÄ±«µÇÁö ¾Êµµ·Ï À¯Áö
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        public delegate void OnSlotCountChange(int value);
        public OnSlotCountChange onSlotCountChange;

        public delegate void OnGetItem();
        public OnGetItem onGetItem;

        public delegate void OnChangeTab(int _index);
        public OnChangeTab onChangeTab;

        public int curTab = 0;
        public List<ItemData>[] itemTabs = new List<ItemData>[3] { new List<ItemData>(), new List<ItemData>(), new List<ItemData>() };

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

        public List<ItemData> GetCurrentTabItems()
        {
            return itemTabs[curTab];
        }

        public void ChangeTab(int _index)
        {
            curTab = _index;
            onChangeTab(_index);
        }

        public bool AddItem(ItemData _item)
        {
            List<ItemData> items = itemTabs[(int)_item.ItemType];
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
            List<ItemData> items = itemTabs[(int)_type];

            if (items.Count <= _index)
                return;

            items.RemoveAt(_index);
            onGetItem();
        }

        private void OnTriggerEnter(Collider collision)
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
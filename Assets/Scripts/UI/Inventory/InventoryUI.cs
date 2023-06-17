using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites.UI.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        Inventory inventory;
        public GameObject InventoryPanel;
        bool isActive = false;

        public Image[] tabImages;
        public Transform tabImageHolder;

        public Slot[] slots;
        public Transform slotHolder;

        private Color Active = new Color32(152, 119, 83, 100);
        private Color nonActive = new Color32(187, 155, 118, 100);

        private void Start()
        {
            inventory = Inventory.Instance;
            tabImages = tabImageHolder.GetComponentsInChildren<Image>();
            slots = slotHolder.GetComponentsInChildren<Slot>();
            inventory.onSlotCountChange += SlotChange;
            inventory.onGetItem += RedrawSlotUI;
            inventory.onChangeTab += ChangeTabUI;
            InventoryPanel.SetActive(isActive);
            inventory.SlotCount = 4;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                isActive = !isActive;
                InventoryPanel.SetActive(isActive);
            }
        }

        private void SlotChange(int value)
        {
            for (int i = 0; i < slots.Length; ++i)
            {
                slots[i].slotNum = i;

                if (i < inventory.SlotCount)
                    slots[i].GetComponent<Button>().interactable = true;
                else
                    slots[i].GetComponent<Button>().interactable = false;
            }
        }

        private void ChangeTabUI(int _index)
        {
            for (int i = 0; i < tabImages.Length; ++i)
            {
                tabImages[i].color = nonActive;
            }

            tabImages[_index].color = Active;

            RedrawSlotUI();
        }

        private void RedrawSlotUI()
        {
            for (int i = 0; i < slots.Length; ++i)
            {
                slots[i].RemoveSlot();
            }

            List<Item> items = inventory.GetCurrentTabItems();
            for (int i = 0; i < items.Count; ++i)
            {
                slots[i].item = items[i];
                slots[i].UpdateSlotUI();
            }
        }

        public void AddSlot()
        {
            ++inventory.SlotCount;
        }
    }
}
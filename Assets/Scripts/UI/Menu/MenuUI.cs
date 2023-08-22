using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites
{
    public class MenuUI : MonoBehaviour
    {
        public GameObject[] contents;

        int currentMenu;

        public void ChangeTab(int menu)
        {
            contents[currentMenu].SetActive(false);
            contents[menu].SetActive(true);

            currentMenu = menu;
        }
    }
}

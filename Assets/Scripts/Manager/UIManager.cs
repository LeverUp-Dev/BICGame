using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites
{
    public class UIManager : MonoBehaviour
    {
        public GameObject sceneCrossFadeUI;
        public GameObject menuUI;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                menuUI.SetActive(!menuUI.activeSelf);
                sceneCrossFadeUI.SetActive(!sceneCrossFadeUI.activeSelf);
            }
        }
    }
}

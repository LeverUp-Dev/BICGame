using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites.UI
{
    public class MapWindow : MonoBehaviour
    {
        public GameObject MapCanvasPanel;
        bool isActive = false;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                isActive = !isActive;
                MapCanvasPanel.SetActive(isActive);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                isActive = false;
                MapCanvasPanel.SetActive(isActive);
            }
        }
    }
}

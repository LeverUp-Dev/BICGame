using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites
{
    public class ClickEvent : MonoBehaviour
    {
        //public Camera mainCamera;
        public static bool zoomActive = false;
        private void OnMouseDown()
        {
            if (!zoomActive) zoomActive = true;
            //if (zoomActive)  zoomActive = false;
            Debug.Log(transform.name);
        }
    }
}

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
            if (!zoomActive && Vector3.Distance(transform.position, Camera.main.transform.position) <= 2f) zoomActive = true;
            Debug.Log(transform.name);
        }
    }
}

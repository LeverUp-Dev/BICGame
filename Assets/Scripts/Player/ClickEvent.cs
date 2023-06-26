using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites
{
    public class ClickEvent : MonoBehaviour
    {
        public Camera mainCamera;
        //private RaycastHit hit;


        private void OnMouseDown()
        {
            Debug.Log(transform.name);
        }
    }
}

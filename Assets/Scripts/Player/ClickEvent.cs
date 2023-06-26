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
        // Update is called once per frame
        void Update()
        {
            /*
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray.origin, ray.direction * 10, out hit))
                {
                    Debug.Log(hit.transform.gameObject.name);
                }
            }*/
        
        }
    }
}

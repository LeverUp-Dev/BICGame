using Hypocrites.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites
{
    public class CameraZoom : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            GetComponent<Camera>().enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if(ClickEvent.zoomActive)
            {
                GetComponent<Camera>().enabled = true;
                PlayerController.cameraOn = true;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GetComponent<Camera>().enabled = false;
                ClickEvent.zoomActive = false;
                PlayerController.cameraOn = false;
            }
        }
    }
}

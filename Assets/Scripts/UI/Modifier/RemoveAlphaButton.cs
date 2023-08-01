using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites
{
    public class RemoveAlphaButton : MonoBehaviour
    {
        void Awake()
        {
            GetComponent<Image>().alphaHitTestMinimumThreshold = 0.9f;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

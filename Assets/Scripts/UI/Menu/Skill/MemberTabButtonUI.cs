using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites
{
    public class MemberTabButtonUI : TabButtonUI
    {
        public GameObject tabNormalFrame;
        public Image memberText;

        public override void SetHighlight(bool highlighting)
        {
            if (highlighting)
                memberText.sprite = highlightTextSprite;
            else
                memberText.sprite = normalTextSprite;

            tabButton.image.enabled = highlighting;
            tabNormalFrame.SetActive(!highlighting);
        }
    }
}

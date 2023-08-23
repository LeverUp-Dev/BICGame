using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hypocrites
{
    public class TabButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Button tabButton;
        public GameObject highlight;
        public GameObject highlightUnderbar;
        public Sprite normalTextSprite;
        public Sprite highlightTextSprite;

        public virtual void SetHighlight(bool highlighting)
        {
            if (tabButton)
            {
                if (highlighting)
                    tabButton.image.sprite = highlightTextSprite;
                else
                    tabButton.image.sprite = normalTextSprite;
            }

            if (highlight)
                highlight.SetActive(highlighting);

            if (highlightUnderbar)
                highlightUnderbar.SetActive(highlighting);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlight(false);
        }
    }
}

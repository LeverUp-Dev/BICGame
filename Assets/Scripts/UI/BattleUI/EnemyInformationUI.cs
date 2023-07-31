using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites.UI.BattleUI
{
    public class EnemyInformationUI : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public RectTransform healthBarRect;

        Transform tr;
        Transform cameraTr;

        float healthBarWidth;

        private void Awake()
        {
            tr = transform;
            cameraTr = Camera.main.transform;

            healthBarWidth = healthBarRect.rect.width;
        }

        private void Update()
        {
            transform.LookAt(cameraTr);
        }

        public void Initialize(string name)
        {
            nameText.text = name;
        }

        public string GetName()
        {
            return nameText.text;
        }

        public void SetHealthBar(float maxHealth, float curHealth)
        {
            healthBarRect.sizeDelta = new Vector2(healthBarWidth * curHealth / maxHealth, healthBarRect.sizeDelta.y);
        }
    }
}

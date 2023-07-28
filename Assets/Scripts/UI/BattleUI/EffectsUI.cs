using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.UI.BattleUI
{
    using Skill;

    public class EffectsUI : MonoBehaviour
    {
        public GameObject effectPrefab;

        Transform tr;

        List<GameObject> effectUIs;
        float effectUIWidth;

        void Awake()
        {
            tr = transform;

            effectUIs = new List<GameObject>();
            effectUIWidth = effectPrefab.GetComponent<RectTransform>().rect.width;
        }

        public void AddEffectUI(Skill effect)
        {
            GameObject effectUI = Instantiate(effectPrefab, tr);
            RectTransform rt = effectUI.GetComponent<RectTransform>();
            rt.anchoredPosition += Vector2.right * (effectUIWidth * effectUIs.Count);

            effectUI.name = effect.Name;

            effectUIs.Add(effectUI);
        }

        public void RemoveEffectUI(Skill effect)
        {
            bool found = false;

            for (int i = 0; i < effectUIs.Count; i++)
            {
                if (effectUIs[i].name == effect.Name)
                {
                    Destroy(effectUIs[i]);
                    effectUIs.RemoveAt(i--);
                    found = true;

                    continue;
                }
                
                // 이펙트가 제거되면 위치 재조정
                if (found)
                {
                    GameObject effectUI = effectUIs[i];
                    RectTransform rt = effectUI.GetComponent<RectTransform>();
                    rt.anchoredPosition += Vector2.left * effectUIWidth;
                }
            }
        }
    }
}

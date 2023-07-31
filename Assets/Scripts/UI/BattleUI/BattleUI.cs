using TMPro;
using UnityEngine;

namespace Hypocrites.UI.BattleUI
{
    using Defines;
    using DB.Data;
    using Skill;

    public class BattleUI : MonoBehaviour
    {
        public EnemyInformationUI[] enemyInformationUIs;
        public MemberInformationUI[] memberInformationUIs;

        Enemy[] enemies;

        void Update()
        {
            if (gameObject.activeSelf && enemies != null)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    Enemy enemy = enemies[i];
                    enemyInformationUIs[i].SetHealthBar(enemy.Status.MaxHealth, enemy.Status.Health);
                }
            }
        }
        
        public void Initialize(Member[] members, Enemy[] enemies)
        {
            foreach (MemberInformationUI memberUI in memberInformationUIs)
                memberUI.gameObject.SetActive(false);

            foreach (EnemyInformationUI enemyUI in enemyInformationUIs)
                enemyUI.gameObject.SetActive(false);

            float memberInformationUIWidth = memberInformationUIs[0].GetComponent<RectTransform>().rect.width + BattleDefines.MEMBER_INFORMATION_PADDING;
            float beginX = -((members.Length - 1) * (memberInformationUIWidth / 2));

            // 멤버 수에 따라 정보 UI 위치 조정
            for (int i = 0; i < members.Length; i++)
            {
                MemberInformationUI memberUI = memberInformationUIs[i];
                memberUI.Initialize(members[i]);
                memberUI.gameObject.SetActive(true);

                RectTransform memberUIRectTransform = memberUI.GetComponent<RectTransform>();
                Vector2 rectPos = memberUIRectTransform.anchoredPosition;
                rectPos.x = beginX + i * memberInformationUIWidth;

                memberUIRectTransform.anchoredPosition = rectPos;

                memberUI.gameObject.name = members[i].Name;
            }

            this.enemies = enemies;
            for (int i = 0; i < enemies.Length; i++)
            {
                Enemy enemy = enemies[i];
                EnemyInformationUI enemyUI = enemyInformationUIs[i];
                enemyUI.Initialize(enemy.Name);
                enemyUI.gameObject.SetActive(true);
                enemyUI.SetHealthBar(enemy.Status.MaxHealth, enemy.Status.Health);

                // TODO 적 위에 자동으로 위치하도록 수정 필요 (몬스터 오브젝트 어떻게 구현할 건지 정하고 나서 작업)
                //float height = enemy.Height;
            }
        }

        public void AddEffectUIToMember(string name, Skill effect)
        {
            MemberInformationUI targetUI = null;

            foreach (MemberInformationUI memberUI in memberInformationUIs)
            {
                if (memberUI.name == name)
                {
                    targetUI = memberUI;
                    break;
                }
            }

            if (targetUI == null)
            {
                //Debug.LogError($"{name} 동료 UI를 Battle")
            }

            targetUI.AddEffectInformation(effect);
        }
    }
}

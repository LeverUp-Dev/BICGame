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

            // ��� ���� ���� ���� UI ��ġ ����
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

                // TODO �� ���� �ڵ����� ��ġ�ϵ��� ���� �ʿ� (���� ������Ʈ ��� ������ ���� ���ϰ� ���� �۾�)
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
                //Debug.LogError($"{name} ���� UI�� Battle")
            }

            targetUI.AddEffectInformation(effect);
        }
    }
}

using TMPro;
using UnityEngine;

namespace Hypocrites.UI.TempBattleUI
{
    using Defines;
    using DB.Data;

    public class BattleUI : MonoBehaviour
    {
        public TextMeshProUGUI enemyNameText;
        public TextMeshProUGUI enemyHealthText;
        public TextMeshProUGUI enemy2NameText;
        public TextMeshProUGUI enemy2HealthText;

        public MemberInformationUI[] memberInformationUIs;

        EnemyData[] enemies;

        float memberInformationUIWidth;

        void Update()
        {
            if (gameObject.activeSelf && enemies != null)
            {
                enemyHealthText.text = enemies[0].Status.Health + "";
                enemy2HealthText.text = enemies[1].Status.Health + "";
            }
        }
        
        public void Initialize(Member[] members, EnemyData[] enemies)
        {
            foreach (MemberInformationUI memberUI in memberInformationUIs)
                memberUI.gameObject.SetActive(false);

            memberInformationUIWidth = memberInformationUIs[0].GetComponent<RectTransform>().rect.width + BattleDefines.MEMBER_INFORMATION_PADDING;
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
            }

            this.enemies = enemies;

            enemyNameText.text = enemies[0].Name;
            enemy2NameText.text = enemies[1].Name;
        }
    }
}

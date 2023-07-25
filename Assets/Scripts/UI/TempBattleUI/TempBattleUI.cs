using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Hypocrites.UI.TempBattleUI
{
    using DB;
    using DB.Data;

    public class TempBattleUI : MonoBehaviour
    {
        public TextMeshProUGUI enemyNameText;
        public TextMeshProUGUI enemyHealthText;
        public TextMeshProUGUI enemy2NameText;
        public TextMeshProUGUI enemy2HealthText;
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI[] skillNameTexts;

        List<EnemyData> enemies;
        Member player;

        // Start is called before the first frame update
        void Start()
        {
            enemies = new List<EnemyData>
            {
                Database.Instance.Enemies[0],
                Database.Instance.Enemies[1]
            };

            player = Database.Instance.Members[0];

            enemyNameText.text = enemies[0].Name;
            enemy2NameText.text = enemies[1].Name;
            playerNameText.text = player.Name;

            for (int i = 0; i < 3; i++)
                skillNameTexts[i].text = player.SkillSlot[i].Name;
        }

        void Update()
        {
            enemyHealthText.text = enemies[0].Status.Health + "";
            enemy2HealthText.text = enemies[1].Status.Health + "";
        }
    }
}

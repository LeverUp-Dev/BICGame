using Hypocrites.DB;
using Hypocrites.DB.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites
{
    public class TempBattleManager : MonoBehaviour
    {
        List<EnemyData> enemies;
        Member player;

        void Start()
        {
            enemies = new List<EnemyData>
            {
                Database.Instance.Enemies[0],
                Database.Instance.Enemies[1]
            };

            player = Database.Instance.Members[0];
        }

        public void UseSkill(int index)
        {
            player.UseSkill(player.SkillSlot[index], new EnemyData[1] { enemies[0] });
        }
    }
}

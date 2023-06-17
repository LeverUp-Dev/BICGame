using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.DB
{
    using DB.Save;
    using DB.Data;
    using UI.Inventory;
    using Utility;

    public class Database : MonoBehaviour
    {
        public static Database Instance { get; private set; }

        public List<Item> Items { get; private set; }
        public List<EnemyData> Enemies { get; private set; }
        public List<PlayerData> Members { get; private set; }

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            /* 아이템 정보 검색 */
            //Items = new JsonSave<Item>();

            /* 적 정보 검색 */
            Enemies = new List<EnemyData>();
            JsonSave<BeingSave> enemyJsonSave = JsonIOUtility.LoadJson<JsonSave<BeingSave>>("Assets/Data/Enemies.json");
            for (int i = 0; i < enemyJsonSave.items.Length; ++i)
            {
                EnemyData e = new EnemyData(enemyJsonSave.items[i]);
                Enemies.Add(e);
            }

            /* 플레이어 및 동료 정보 검색 */
            Members = new List<PlayerData>();
            JsonSave<PlayerSave> membersSave = JsonIOUtility.LoadJson<JsonSave<PlayerSave>>("Assets/Data/Members.json");
            for (int i = 0; i < membersSave.items.Length; i++)
            {
                PlayerData data = new PlayerData(membersSave.items[i]);
                Members.Add(data);
            }
        }
    }
}

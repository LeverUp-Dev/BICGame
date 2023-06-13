using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.DB
{
    using DB.Save;
    using DB.Data;
    using Inventory;
    using Utility;

    public class Database : MonoBehaviour
    {
        public static Database Instance { get; private set; }

        public List<Item> Items { get; private set; }
        public List<EnemyData> Enemies { get; private set; }

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
        }
    }
}

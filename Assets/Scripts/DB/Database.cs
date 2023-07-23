using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.DB
{
    using DB.Save;
    using DB.Data;
    using UI.Inventory;
    using Utility;
    using Defines;
    using Skill;

    public class Database : MonoBehaviour
    {
        public static Database Instance { get; private set; }

        [field: SerializeField] public List<ItemData> Items { get; private set; }
        public List<EnemyData> Enemies { get; private set; }
        public List<Member> Members { get; private set; }
        public Dictionary<string, Skill> Skills { get; private set; }

        /* Inspector Values */
        public GameObject fieldItemPrefab;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            /* 아이템 정보 검색 */
            Items = new List<ItemData>();
            JsonSave<ItemSave> itemJsonSave = JsonIOUtility.LoadJson<JsonSave<ItemSave>>(DatabaseConstants.ITEM_DATA_PATH);
            for (int i = 0; i < itemJsonSave.items.Length; ++i)
            {
                ItemData item = new ItemData(itemJsonSave.items[i]);
                Items.Add(item);

                GameObject go = Instantiate(fieldItemPrefab, new Vector3((i + 1) * 2, 1, 0), Quaternion.identity);
                go.GetComponent<FieldItems>().SetItem(item);
            }

            /* 스킬 정보 검색 */
            Skills = new Dictionary<string, Skill>();
            JsonSave<SkillSave> skillsSave = JsonIOUtility.LoadJson<JsonSave<SkillSave>>(DatabaseConstants.SKILL_DATA_PATH);
            for (int i = 0; i < skillsSave.items.Length; i++)
            {
                SkillSave save = skillsSave.items[i];
                Skill skill = new Skill(save);

                Skills.Add(skill.Name, skill);
            }

            /* 적 정보 검색 */
            Enemies = new List<EnemyData>();
            JsonSave<BeingSave> enemyJsonSave = JsonIOUtility.LoadJson<JsonSave<BeingSave>>(DatabaseConstants.ENEMY_DATA_PATH);
            for (int i = 0; i < enemyJsonSave.items.Length; ++i)
            {
                EnemyData e = new EnemyData(enemyJsonSave.items[i]);
                Enemies.Add(e);
            }

            /* 플레이어 및 동료 정보 검색 */
            Members = new List<Member>();
            JsonSave<MemberSave> membersSave = JsonIOUtility.LoadJson<JsonSave<MemberSave>>(DatabaseConstants.MEMBER_DATA_PATH);
            for (int i = 0; i < membersSave.items.Length; i++)
            {
                Member data = new Member(membersSave.items[i]);
                Members.Add(data);
            }
        }

        public void SaveMembers()
        {
            JsonSave<MemberSave> membersSave = new JsonSave<MemberSave>();
            membersSave.items = new MemberSave[Members.Count];

            for (int i = 0; i < Members.Count; ++i)
            {
                Member data = Members[i];
                MemberSave save = new MemberSave(data);

                membersSave.items[i] = save;
            }

            JsonIOUtility.SaveJson(DatabaseConstants.MEMBER_DATA_PATH, membersSave);
        }
    }
}

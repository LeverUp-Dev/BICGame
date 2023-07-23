using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.Defines
{
    public enum ItemType
    {
        Equipment,
        Artifact,
        Consumable
    }

    public class DatabaseConstants
    {
        public const string ITEM_DATA_PATH = "Assets/Data/Items.json";
        public const string ENEMY_DATA_PATH = "Assets/Data/Enemies.json";
        public const string MEMBER_DATA_PATH = "Assets/Data/Members.json";
        public const string SKILL_DATA_PATH = "Assets/Data/Skills.json";
    }
}

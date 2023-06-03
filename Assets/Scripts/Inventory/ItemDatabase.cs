using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.Inventory
{
    public class ItemDatabase : MonoBehaviour
    {
        public static ItemDatabase instance;
        private void Awake()
        {
            instance = this;
        }

        public List<Item> db = new List<Item>();

        public GameObject fieldItemPrefab;
        public Vector3[] pos;

        private void Start()
        {
            for (int i = 0; i < 3; ++i)
            {
                GameObject go = Instantiate(fieldItemPrefab, pos[i], Quaternion.identity);
                go.GetComponent<FieldItems>().SetItem(db[Random.Range(0, 3)]);
            }
        }
    }
}
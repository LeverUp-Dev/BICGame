using Hypocrites.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites
{
    public class MonsterVannish : MonoBehaviour
    {
        // Start is called before the first frame update
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                BattleManager.Instance.BeginBattle();
                Destroy(gameObject, 1);
            }
        }
    }
}

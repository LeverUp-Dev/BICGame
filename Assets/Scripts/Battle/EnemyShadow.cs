using Hypocrites.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hypocrites.Battle
{
    public class EnemyShadow : MonoBehaviour
    {
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

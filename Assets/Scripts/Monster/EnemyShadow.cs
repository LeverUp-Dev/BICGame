using Hypocrites.Grid;
using Hypocrites.Manager;
using Hypocrites.Player;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Hypocrites.Battle
{
    public class EnemyShadow : MonoBehaviour
    {
        public enum EnemyState { idle, chase, attack }; // 열거형 선언
        public EnemyState enemyState = EnemyState.idle; // 변수 선언 및 초기화
        public Animator anim;
        bool isChase = false;

        public float speed = 1f;
        public GameObject player;
        Vector3 prevPos;

        void Awake()
        {
            anim = GetComponent<Animator>();
        }
        void Update()
        {
            StartCoroutine(CheckEnemy());
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))isChase = true;
        }
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) isChase = false;
        }
        IEnumerator CheckEnemy()
        {
            float dist = Vector3.Distance(player.transform.position, transform.position);

            if (isChase)
            {
                enemyState = EnemyState.chase;
                anim.SetBool("IsChase", true);
            }
            else
            {
                enemyState = EnemyState.idle;
                anim.SetBool("IsChase", false);
            }

            switch (enemyState)
            {
                case EnemyState.chase:
                    //print("Chase");
                    Vector3 direction = player.transform.position - transform.position;
                    direction.Normalize();
                    transform.forward = direction;
                    transform.position += direction * Time.deltaTime * speed;
                    break;
                default:
                    //print("idle");
                    break;
            }
            yield return new WaitForSeconds(1f);

        }
        /*
        IEnumerator EnemyAction()
        {
            while (true)
            {
                switch (enemyState)
                {
                    case EnemyState.chase:
                        print("Chase");
                        if (true)
                        {
                            prevPos = Vector3Int.RoundToInt(player.position);
                            Vector3 direction = player.position - transform.position;
                            direction.Normalize();
                            transform.position += direction * speed * Time.deltaTime;
                        }
                        break;
                    default:
                        print("idle");
                        break;
                }
                yield return null;
            }
        }*/
    }
}

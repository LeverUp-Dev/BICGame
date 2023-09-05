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
    using Defines;

    public class EnemyShadow : MonoBehaviour
    {
        public enum EnemyState { idle, chase, attack }; // 열거형 선언
        public EnemyState enemyState = EnemyState.idle; // 변수 선언 및 초기화
        public Animator anim;

        public float speed = 1f;
        public GameObject player;

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
            //if (other.CompareTag("Player")) isChase = true;
            enemyState = EnemyState.chase;
            anim.SetBool("IsChase", true);
        }
        void OnTriggerExit(Collider other)
        {
            //if (other.CompareTag("Player")) isChase = false;
            enemyState = EnemyState.idle;
            anim.SetBool("IsChase", false);
        }
        IEnumerator CheckEnemy()
        {
            float dist = Vector3.Distance(player.transform.position, transform.position);
            /*
            if (isChase)
            {
                enemyState = EnemyState.chase;
                anim.SetBool("IsChase", true);
            }
            else
            {
                enemyState = EnemyState.idle;
                anim.SetBool("IsChase", false);
            }*/

            Vector3 direction;
            switch (enemyState)
            {
                case EnemyState.chase:
                    //print("Chase");
                    direction = player.transform.position - transform.position;
                    direction.Normalize();
                    transform.forward = direction;
                    transform.position += direction * Time.deltaTime * speed;
                    break;
                default:
                    direction = transform.position;
                    List<Directions> dir = new List<Directions>();
                    if (CGrid.Instance.GetNodeFromWorldPosition(direction - Vector3Int.forward * 2).Walkable) dir.Add(Directions.DOWN);
                    if (CGrid.Instance.GetNodeFromWorldPosition(direction + Vector3Int.right * 2).Walkable) dir.Add(Directions.RIGHT);
                    if (CGrid.Instance.GetNodeFromWorldPosition(direction + Vector3Int.forward * 2).Walkable) dir.Add(Directions.UP);
                    if (CGrid.Instance.GetNodeFromWorldPosition(direction - Vector3Int.right * 2).Walkable) dir.Add(Directions.LEFT);

                    Directions rand = dir[Random.Range(0, dir.Count)];
                    if (rand == Directions.DOWN || rand == Directions.UP) 
                        direction += Vector3Int.forward * (rand == Directions.UP ? 1 : -1) * 2;
                    else direction += Vector3Int.right * (rand == Directions.RIGHT ? 1 : -1) * 2;

                    direction -= transform.position;
                    direction.Normalize();
                    transform.forward = direction;
                    transform.position += direction * Time.deltaTime * speed  * 2;
                    //print("idle");
                    break;
            }
            yield return new WaitForSeconds(5f);

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

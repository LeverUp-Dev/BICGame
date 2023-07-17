using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hypocrites
{
    public class SceneChanger : MonoBehaviour
    {
        public Animator crossFade;
        public string scenename;

        IEnumerator LoadBattleField()
        {
            crossFade.SetTrigger("Start");
            yield return new WaitForSeconds(1f);
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                StartCoroutine(LoadBattleField());
                SceneManager.LoadScene(scenename);
            }
        }
    }
}

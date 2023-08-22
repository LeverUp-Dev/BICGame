using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hypocrites.Manager
{
    using Map;

    public class GameSceneManager : MonoBehaviour
    {
        public static GameSceneManager Instance { get; private set; }

        public Animator crossFade;

        public GameObject[] mainSceneActivationTargets;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string name, bool isAdditive = false)
        {
            StartCoroutine(TriggerCrossFadeCoroutine());

            StartCoroutine(LoadSceneCoroutine(name, isAdditive));
        }

        IEnumerator LoadSceneCoroutine(string name, bool isAdditive)
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(name, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            // ���� ������ �ε�� ������ ���
            while (!async.isDone)
                yield return new WaitForEndOfFrame();

            // ���� �ε�� ���� ActiveScene���� ����
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));

            ToggleStatus();
        }

        public void UnloadScene(string name)
        {
            StartCoroutine(TriggerCrossFadeCoroutine());

            SceneManager.UnloadSceneAsync(name);

            ToggleStatus();
        }

        IEnumerator TriggerCrossFadeCoroutine()
        {
            crossFade.SetTrigger("Start");
            yield break;
        }

        void ToggleStatus()
        {
            GameState state = GameStateManager.Instance.state;

            if (state == GameState.OnBattle)
                GameStateManager.Instance.state = GameState.OnPlay;
            else
                GameStateManager.Instance.state = GameState.OnBattle;

            foreach (GameObject obj in mainSceneActivationTargets)
            {
                if (obj != null)
                    obj.SetActive(!obj.activeSelf);
            }
        }
    }
}

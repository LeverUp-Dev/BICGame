using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnClickNewGame()
        {
            Debug.Log("새 게임");
        }
        public void OnClickLoad()
        {
            Debug.Log("불러오기");
        }
        public void OnClickSetting()
        {
            Debug.Log("설정");
        }
        public void OnClickClosedGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
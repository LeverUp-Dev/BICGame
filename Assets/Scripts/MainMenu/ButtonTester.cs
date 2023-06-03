using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites.MainMenu
{
    public class ButtonTester : MonoBehaviour
    {
        Button button;

        public void OnClickButton()
        {
            Debug.Log("Button Click!!");
        }
        // Start is called before the first frame update
        void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClickButton);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

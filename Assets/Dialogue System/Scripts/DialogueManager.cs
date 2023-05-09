using System.Collections;
using UnityEngine;
using TMPro;

using DS;
using DS.Data;
using DS.Enumerations;
using DS.ScriptableObjects;


public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance = null;

    public GameObject DialogueCanvas;
    public TextMeshProUGUI textDisplay;
    public GameObject continueButton;
    public GameObject[] choiceButtons;
    public float typingSpeed;

    private DSDialogueSO currentDialogue = null;
    private bool isLastDialogue = false;

    public static DialogueManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            // Scene 전환 시 파괴되지 않게 하려면 활성화
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Scene 이동 후 해당 scene의 Hierarchy에 DialogueManager가 있다면 이전 scene의 DialogueManager를 사용하기 위해 Destroy 수행
            Destroy(gameObject);
        }
    }

    /*
     * 대화 UI의 Enabled 상태 설정
     */
    public void SetEnabled(bool isEnabled)
    {
        DialogueCanvas.SetActive(isEnabled);

        if (!isEnabled)
        {
            textDisplay.text = "";

            continueButton.GetComponent<TextMeshProUGUI>().text = "";

            foreach (GameObject button in choiceButtons)
            {
                button.GetComponent<TextMeshProUGUI>().text = "";
            }
        }
    }

    /*
     * DialogueManager 동작 순서
     * 1. DSDialogue를 가지고 있는 Object가 충돌을 감지하면 DialogueManager의 SetDialogue()를 호출해 dialogue 데이터 전달
     * 2. Dialogue Manager는 대화 UI를 활성화하고 대화 출력
     * 3. 대화 UI에 있는 다음 또는 선택지 버튼을 클릭하면 Next()를 호출
     * 4. 마지막 대화까지 출력하고 나서 플레이어가 종료 버튼을 클릭하면 대화 UI 비활성화
     */
    public void SetDialogue(DSDialogueSO dialogue)
    {
        isLastDialogue = false;
        currentDialogue = dialogue;

        Type();
    }

    /*
     * 대화 UI에 대사를 한 글자씩 출력하는 Coroutine 실행
     */
    public void Type()
    {
        isLastDialogue = false;

        if (currentDialogue.DialogueType == DSDialogueType.SingleChoice)
        {
            DSDialogueSO nextDialogue = currentDialogue.Choices[0].NextDialogue;

            // 다음/종료 버튼 문구 설정
            if (nextDialogue == null)
            {
                continueButton.GetComponent<TextMeshProUGUI>().text = "종료";
                isLastDialogue = true;
            }
            else
            {
                continueButton.GetComponent<TextMeshProUGUI>().text = "다음";
            }
        }
        else
        {
            // 다중 선택지인 경우 다음 Dialogue가 반드시 있어야 하므로 null이 존재한다면 Type을 수행하지 않음
            foreach (DSDialogueChoiceData nextChoice in currentDialogue.Choices)
            {
                if (nextChoice.NextDialogue == null)
                {
                    Debug.LogError($"{currentDialogue.DialogueName} 대화에 다음 대화가 지정되지 않은 선택지가 있습니다.");
                    return;
                }
            }

            //continueButton.SetActive(false);

            // 선택지 버튼 문구 설정
            for (int i = 0; i < currentDialogue.Choices.Count; ++i)
            {
                choiceButtons[i].GetComponent<TextMeshProUGUI>().text = currentDialogue.Choices[i].Text;
            }
        }

        SetEnabled(true);

        StartCoroutine(_Type());
    }

    /*
     * 대사 출력 Coroutine 
     */
    IEnumerator _Type()
    {
        foreach (char letter in currentDialogue.Text.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(0.02f);
        }

        if (currentDialogue.DialogueType == DSDialogueType.SingleChoice)
        {
            continueButton.SetActive(true);
        }
        else
        {
            for (int i = 0; i < currentDialogue.Choices.Count; ++i)
            {
                choiceButtons[i].SetActive(true);
            }
        }
    }

    /*
     * 다음 대사로 데이터를 바꾼 뒤 Type() 호출
     */
    public void Next(int choice = 0)
    {
        // isLastDialogue가 true면 이미 마지막 대화를 출력한 상태이므로 SetActive(false) 수행 후 종료
        if (isLastDialogue)
        {
            SetEnabled(false);
            return;
        }

        if (choice > 0)
        {
            if (currentDialogue.DialogueType == DSDialogueType.SingleChoice)
            {
                return;
            }

            if (choice > currentDialogue.Choices.Count - 1)
            {
                return;
            }
        }

        // 대사 출력이 끝나기 전까진 다음 또는 선택지 버튼을 모두 비활성화
        if (currentDialogue.DialogueType == DSDialogueType.SingleChoice)
        {
            continueButton.SetActive(false);
        }
        else
        {
            foreach (GameObject button in choiceButtons)
            {
                button.SetActive(false);
            }
        }

        textDisplay.text = "";

        // 다음 대사로 데이터 변경
        currentDialogue = currentDialogue.Choices[choice].NextDialogue;

        if (currentDialogue == null)
        {
            return;
        }

        Type();
    }
}

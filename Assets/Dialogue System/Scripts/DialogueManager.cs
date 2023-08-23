using System.Collections;
using UnityEngine;
using TMPro;
using DS.Data;
using DS.Enumerations;
using DS.ScriptableObjects;
using UnityEngine.Events;

public class DialogueManager : SingletonMono<DialogueManager>
{
    [field: SerializeField] public GameObject DialogueCanvas { get; private set; }
    [field: SerializeField] public TextMeshProUGUI TextDisplay { get; private set; }
    [field: SerializeField] public GameObject ContinueButton { get; private set; }
    [field: SerializeField] public GameObject[] ChoiceButtons { get; private set; }
    [field: SerializeField] public float TypingSpeed { get; private set; }
    [field: SerializeField] public SerializableDictionary<string, UnityEvent> DialogueEvents { get; set; }
    public bool IsTyping { get; private set; }

    private DSDialogueSO currentDialogue = null;
    private bool isLastDialogue = false;

    protected override void Awake()
    {
        base.Awake();

        IsTyping = false;
    }

    /*
     * 대화 UI의 Enabled 상태 설정
     */
    public void SetEnabled(bool isEnabled)
    {
        IsTyping = isEnabled;
        DialogueCanvas.SetActive(isEnabled);

        if (!isEnabled)
        {
            TextDisplay.text = "";

            ContinueButton.SetActive(false);
            ContinueButton.GetComponent<TextMeshProUGUI>().text = "";

            foreach (GameObject button in ChoiceButtons)
            {
                button.SetActive(false);
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
        if (IsTyping)
            return;

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
                ContinueButton.GetComponent<TextMeshProUGUI>().text = "종료";
                isLastDialogue = true;
            }
            else
            {
                ContinueButton.GetComponent<TextMeshProUGUI>().text = "다음";
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

            // 선택지 버튼 문구 설정
            for (int i = 0; i < currentDialogue.Choices.Count; ++i)
            {
                ChoiceButtons[i].GetComponent<TextMeshProUGUI>().text = currentDialogue.Choices[i].Text;
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
            TextDisplay.text += letter;
            yield return new WaitForSeconds(0.02f);
        }

        // 다이얼로그 출력이 끝나면 할당된 이벤트 호출
        if (DialogueEvents.ContainsKey(currentDialogue.name)) {
            DialogueEvents[currentDialogue.name].Invoke();
        }

        if (currentDialogue.DialogueType == DSDialogueType.SingleChoice)
        {
            ContinueButton.SetActive(true);
        }
        else
        {
            for (int i = 0; i < currentDialogue.Choices.Count; ++i)
            {
                ChoiceButtons[i].SetActive(true);
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
            ContinueButton.SetActive(false);
        }
        else
        {
            foreach (GameObject button in ChoiceButtons)
            {
                button.SetActive(false);
            }
        }

        TextDisplay.text = "";

        // 다음 대사로 데이터 변경
        currentDialogue = currentDialogue.Choices[choice].NextDialogue;

        if (currentDialogue == null)
        {
            return;
        }

        Type();
    }
}
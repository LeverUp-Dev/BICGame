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

    public void SetEnabled(bool isEnabled)
    {
        DialogueCanvas.SetActive(isEnabled);
    }

    public void SetDialogue(DSDialogueSO dialogue)
    {
        isLastDialogue = false;
        currentDialogue = dialogue;

        Type();
    }

    public void Type()
    {
        if (currentDialogue.DialogueType == DSDialogueType.SingleChoice)
        {
            DSDialogueSO nextDialogue = currentDialogue.Choices[0].NextDialogue;

            if (nextDialogue == null)
            {
                continueButton.GetComponent<TextMeshProUGUI>().text = "종료";
                isLastDialogue = true;
            }
            else
            {
                continueButton.GetComponent<TextMeshProUGUI>().text = "다음";
                isLastDialogue = false;
            }
        }
        else
        {
            // 다중 선택지인 경우 다음 Dialogue가 반드시 있어야 하므로 null이 존재한다면 Type을 수행하지 않음
            foreach (DSDialogueChoiceData nextChoice in currentDialogue.Choices)
            {
                if (nextChoice.NextDialogue == null)
                {
                    Debug.LogWarning($"{currentDialogue.DialogueName} 대화에 다음 대화가 지정되지 않은 선택지가 있습니다.");
                    return;
                }
            }

            continueButton.GetComponent<TextMeshProUGUI>().text = "다음";
            isLastDialogue = true;
        }

        SetEnabled(true);

        StartCoroutine(_Type());
    }

    IEnumerator _Type()
    {
        foreach (char letter in currentDialogue.Text.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(0.02f);
        }

        continueButton.SetActive(true);
    }

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

        continueButton.SetActive(false);

        textDisplay.text = "";

        currentDialogue = currentDialogue.Choices[choice].NextDialogue;

        if (currentDialogue == null)
        {
            return;
        }

        Type();
    }
}

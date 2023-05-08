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

            // Scene ��ȯ �� �ı����� �ʰ� �Ϸ��� Ȱ��ȭ
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Scene �̵� �� �ش� scene�� Hierarchy�� DialogueManager�� �ִٸ� ���� scene�� DialogueManager�� ����ϱ� ���� Destroy ����
            Destroy(gameObject);
        }
    }

    /*
     * ��ȭ UI�� Enabled ���� ����
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
     * DialogueManager ���� ����
     * 1. DSDialogue�� ������ �ִ� Object�� �浹�� �����ϸ� DialogueManager�� SetDialogue()�� ȣ���� dialogue ������ ����
     * 2. Dialogue Manager�� ��ȭ UI�� Ȱ��ȭ�ϰ� ��ȭ ���
     * 3. ��ȭ UI�� �ִ� ���� �Ǵ� ������ ��ư�� Ŭ���ϸ� Next()�� ȣ��
     * 4. ������ ��ȭ���� ����ϰ� ���� �÷��̾ ���� ��ư�� Ŭ���ϸ� ��ȭ UI ��Ȱ��ȭ
     */
    public void SetDialogue(DSDialogueSO dialogue)
    {
        isLastDialogue = false;
        currentDialogue = dialogue;

        Type();
    }

    /*
     * ��ȭ UI�� ��縦 �� ���ھ� ����ϴ� Coroutine ����
     */
    public void Type()
    {
        isLastDialogue = false;

        if (currentDialogue.DialogueType == DSDialogueType.SingleChoice)
        {
            DSDialogueSO nextDialogue = currentDialogue.Choices[0].NextDialogue;

            // ����/���� ��ư ���� ����
            if (nextDialogue == null)
            {
                continueButton.GetComponent<TextMeshProUGUI>().text = "����";
                isLastDialogue = true;
            }
            else
            {
                continueButton.GetComponent<TextMeshProUGUI>().text = "����";
            }
        }
        else
        {
            // ���� �������� ��� ���� Dialogue�� �ݵ�� �־�� �ϹǷ� null�� �����Ѵٸ� Type�� �������� ����
            foreach (DSDialogueChoiceData nextChoice in currentDialogue.Choices)
            {
                if (nextChoice.NextDialogue == null)
                {
                    Debug.LogError($"{currentDialogue.DialogueName} ��ȭ�� ���� ��ȭ�� �������� ���� �������� �ֽ��ϴ�.");
                    return;
                }
            }

            //continueButton.SetActive(false);

            // ������ ��ư ���� ����
            for (int i = 0; i < currentDialogue.Choices.Count; ++i)
            {
                choiceButtons[i].GetComponent<TextMeshProUGUI>().text = currentDialogue.Choices[i].Text;
            }
        }

        SetEnabled(true);

        StartCoroutine(_Type());
    }

    /*
     * ��� ��� Coroutine 
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
     * ���� ���� �����͸� �ٲ� �� Type() ȣ��
     */
    public void Next(int choice = 0)
    {
        // isLastDialogue�� true�� �̹� ������ ��ȭ�� ����� �����̹Ƿ� SetActive(false) ���� �� ����
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

        // ��� ����� ������ ������ ���� �Ǵ� ������ ��ư�� ��� ��Ȱ��ȭ
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

        // ���� ���� ������ ����
        currentDialogue = currentDialogue.Choices[choice].NextDialogue;

        if (currentDialogue == null)
        {
            return;
        }

        Type();
    }
}

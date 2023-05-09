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

            // Scene ��ȯ �� �ı����� �ʰ� �Ϸ��� Ȱ��ȭ
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Scene �̵� �� �ش� scene�� Hierarchy�� DialogueManager�� �ִٸ� ���� scene�� DialogueManager�� ����ϱ� ���� Destroy ����
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
                continueButton.GetComponent<TextMeshProUGUI>().text = "����";
                isLastDialogue = true;
            }
            else
            {
                continueButton.GetComponent<TextMeshProUGUI>().text = "����";
                isLastDialogue = false;
            }
        }
        else
        {
            // ���� �������� ��� ���� Dialogue�� �ݵ�� �־�� �ϹǷ� null�� �����Ѵٸ� Type�� �������� ����
            foreach (DSDialogueChoiceData nextChoice in currentDialogue.Choices)
            {
                if (nextChoice.NextDialogue == null)
                {
                    Debug.LogWarning($"{currentDialogue.DialogueName} ��ȭ�� ���� ��ȭ�� �������� ���� �������� �ֽ��ϴ�.");
                    return;
                }
            }

            continueButton.GetComponent<TextMeshProUGUI>().text = "����";
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

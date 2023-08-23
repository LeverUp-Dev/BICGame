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
     * ��ȭ UI�� Enabled ���� ����
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
     * DialogueManager ���� ����
     * 1. DSDialogue�� ������ �ִ� Object�� �浹�� �����ϸ� DialogueManager�� SetDialogue()�� ȣ���� dialogue ������ ����
     * 2. Dialogue Manager�� ��ȭ UI�� Ȱ��ȭ�ϰ� ��ȭ ���
     * 3. ��ȭ UI�� �ִ� ���� �Ǵ� ������ ��ư�� Ŭ���ϸ� Next()�� ȣ��
     * 4. ������ ��ȭ���� ����ϰ� ���� �÷��̾ ���� ��ư�� Ŭ���ϸ� ��ȭ UI ��Ȱ��ȭ
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
                ContinueButton.GetComponent<TextMeshProUGUI>().text = "����";
                isLastDialogue = true;
            }
            else
            {
                ContinueButton.GetComponent<TextMeshProUGUI>().text = "����";
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

            // ������ ��ư ���� ����
            for (int i = 0; i < currentDialogue.Choices.Count; ++i)
            {
                ChoiceButtons[i].GetComponent<TextMeshProUGUI>().text = currentDialogue.Choices[i].Text;
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
            TextDisplay.text += letter;
            yield return new WaitForSeconds(0.02f);
        }

        // ���̾�α� ����� ������ �Ҵ�� �̺�Ʈ ȣ��
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

        // ���� ���� ������ ����
        currentDialogue = currentDialogue.Choices[choice].NextDialogue;

        if (currentDialogue == null)
        {
            return;
        }

        Type();
    }
}
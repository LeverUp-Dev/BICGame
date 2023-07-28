using UnityEngine;

namespace DS
{
    using Hypocrites.Player;
    using ScriptableObjects;

    public class DSDialogue : MonoBehaviour
    {
        [SerializeField] private bool isTrigger;

        /* Dialogue Scriptable Objects */
        [SerializeField] private DSDialogueContainerSO dialogueContainer;
        [SerializeField] private DSDialogueGroupSO dialogueGroup;
        [SerializeField] private DSDialogueSO dialogue;

        /* Filters */
        [SerializeField] private bool groupedDialogues;
        [SerializeField] private bool startingDialoguesOnly;

        /* Indexes */
        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;

        /*
        private void OnTriggerEnter(Collider other)
        {

            if (isTrigger)
            {
                if (other.gameObject.CompareTag("Player"))
                {
                    DialogueManager.Instance.SetDialogue(dialogue);
                }
            }
        }*/

        private void OnMouseDown()
        {
            if(isTrigger && Vector3.Distance(transform.position, Camera.main.transform.position) <= 1.95f)
                DialogueManager.Instance.SetDialogue(dialogue);
        }

        public DSDialogueContainerSO GetDialogueContainer()
        {
            return dialogueContainer;
        }
    }
}
